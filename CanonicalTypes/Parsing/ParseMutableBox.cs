using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sunlighter.OptionLib;

namespace Sunlighter.CanonicalTypes.Parsing
{
    public static partial class Parser
    {
        public static ImmutableHashSet<T> UnionAll<T>(this IEnumerable<ImmutableHashSet<T>> sets)
        {
            ImmutableHashSet<T> result = ImmutableHashSet<T>.Empty;
            foreach (var set in sets)
            {
                result = result.Union(set);
            }
            return result;
        }

        public static ImmutableDictionary<K, ImmutableList<V>> UnionAll<K, V>(this IEnumerable<ImmutableDictionary<K, ImmutableList<V>>> dictionaries)
        {
            ImmutableDictionary<K, ImmutableList<V>> result = ImmutableDictionary<K, ImmutableList<V>>.Empty;
            foreach (var dictionary in dictionaries)
            {
                foreach (var kvp in dictionary)
                {
                    result = result.SetItem(kvp.Key, (result.ContainsKey(kvp.Key) ? result[kvp.Key] : ImmutableList<V>.Empty).AddRange(kvp.Value));
                }
            }
            return result;
        }

        public static ImmutableDictionary<K, ImmutableList<V>> Union<K, V>(this ImmutableDictionary<K, ImmutableList<V>> d1, ImmutableDictionary<K, ImmutableList<V>> d2)
        {
            var result = d1;
            foreach (var kvp in d2)
            {
                result = result.SetItem(kvp.Key, (result.ContainsKey(kvp.Key) ? result[kvp.Key] : ImmutableList<V>.Empty).AddRange(kvp.Value));
            }
            return result;
        }

        private interface IDatumBuilder
        {
            ImmutableHashSet<int> BoxReferences { get; }

            ImmutableDictionary<int, ImmutableList<IDatumBuilder>> BoxValues { get; }

            Datum Build(ImmutableDictionary<int, MutableBoxDatum> boxes);
        }

        private class AtomBuilder : IDatumBuilder
        {
            private readonly Datum value;

            public AtomBuilder(Datum value)
            {
                this.value = value;
            }

            public ImmutableHashSet<int> BoxReferences
            {
                get
                {
                    return ImmutableHashSet<int>.Empty;
                }
            }

            public ImmutableDictionary<int, ImmutableList<IDatumBuilder>> BoxValues
            {
                get
                {
                    return ImmutableDictionary<int, ImmutableList<IDatumBuilder>>.Empty;
                }
            }

            public Datum Build(ImmutableDictionary<int, MutableBoxDatum> boxes)
            {
                return value;
            }
        }

        private class BoxBuilder : IDatumBuilder
        {
            private readonly int? key;
            private readonly Option<IDatumBuilder> value;

            public BoxBuilder(int? key, Option<IDatumBuilder> value)
            {
                if (!key.HasValue && !value.HasValue)
                {
                    throw new ArgumentException("BoxBuilder requires key, value, or both to be provided");
                }

                this.key = key;
                this.value = value;
            }

            public ImmutableHashSet<int> BoxReferences
            {
                get
                {
                    var result = ImmutableHashSet<int>.Empty;
                    if (key.HasValue)
                    {
                        result = result.Add(key.Value);
                    }
                    if (value.HasValue)
                    {
                        result = result.Union(value.Value.BoxReferences);
                    }
                    return result;
                }
            }

            public ImmutableDictionary<int, ImmutableList<IDatumBuilder>> BoxValues
            {
                get
                {
                    var result = ImmutableDictionary<int, ImmutableList<IDatumBuilder>>.Empty;
                    if (key.HasValue && value.HasValue)
                    {
                        result = result.Add(key.Value, ImmutableList<IDatumBuilder>.Empty.Add(value.Value));
                    }
                    if (value.HasValue)
                    {
                        result = result.Union(value.Value.BoxValues);
                    }
                    return result;
                }
            }

            public Datum Build(ImmutableDictionary<int, MutableBoxDatum> boxes)
            {
                if (key.HasValue)
                {
                    return boxes[key.Value];
                }
                else
                {
                    return new MutableBoxDatum(value.Value.Build(boxes));
                }
            }
        }

        private class ListBuilder : IDatumBuilder
        {
            private readonly ImmutableList<IDatumBuilder> values;

            public ListBuilder(ImmutableList<IDatumBuilder> values)
            {
                this.values = values;
            }

            public ImmutableHashSet<int> BoxReferences
            {
                get
                {
                    return values.Select(v => v.BoxReferences).UnionAll();
                }
            }

            public ImmutableDictionary<int, ImmutableList<IDatumBuilder>> BoxValues
            {
                get
                {
                    return values.Select(v => v.BoxValues).UnionAll();
                }
            }

            public Datum Build(ImmutableDictionary<int, MutableBoxDatum> boxes)
            {
                ListDatum result = ListDatum.Empty;
                foreach(IDatumBuilder value in values)
                {
                    result = result.Add(value.Build(boxes));
                }
                return result;
            }
        }

        private class SetBuilder : IDatumBuilder
        {
            private readonly ImmutableList<IDatumBuilder> values;

            public SetBuilder(ImmutableList<IDatumBuilder> values)
            {
                this.values = values;
            }

            public ImmutableHashSet<int> BoxReferences
            {
                get
                {
                    return values.Select(v => v.BoxReferences).UnionAll();
                }
            }

            public ImmutableDictionary<int, ImmutableList<IDatumBuilder>> BoxValues
            {
                get
                {
                    return values.Select(v => v.BoxValues).UnionAll();
                }
            }

            public Datum Build(ImmutableDictionary<int, MutableBoxDatum> boxes)
            {
                SetDatum result = SetDatum.Empty;
                foreach (IDatumBuilder value in values)
                {
                    result = result.Add(value.Build(boxes));
                }
                return result;
            }

            private static Lazy<SetBuilder> empty = new Lazy<SetBuilder>(() => new SetBuilder(ImmutableList<IDatumBuilder>.Empty), LazyThreadSafetyMode.ExecutionAndPublication);

            public static SetBuilder Empty => empty.Value;

            public SetBuilder Add(IDatumBuilder item)
            {
                return new SetBuilder(values.Add(item));
            }
        }

        private class DictionaryBuilder : IDatumBuilder
        {
            private readonly ImmutableList<Tuple<IDatumBuilder, IDatumBuilder>> values;

            public DictionaryBuilder(ImmutableList<Tuple<IDatumBuilder, IDatumBuilder>> values)
            {
                this.values = values;
            }

            public ImmutableHashSet<int> BoxReferences
            {
                get
                {
                    return values.SelectMany(v => new[] { v.Item1.BoxReferences, v.Item2.BoxReferences }).UnionAll();
                }
            }

            public ImmutableDictionary<int, ImmutableList<IDatumBuilder>> BoxValues
            {
                get
                {
                    return values.SelectMany(v => new[] { v.Item1.BoxValues, v.Item2.BoxValues }).UnionAll();
                }
            }

            public Datum Build(ImmutableDictionary<int, MutableBoxDatum> boxes)
            {
                DictionaryDatum result = DictionaryDatum.Empty;
                foreach (Tuple<IDatumBuilder, IDatumBuilder> value in values)
                {
                    result = result.Add(value.Item1.Build(boxes), value.Item2.Build(boxes));
                }
                return result;
            }

            private static Lazy<DictionaryBuilder> empty = new Lazy<DictionaryBuilder>
            (
                () => new DictionaryBuilder(ImmutableList<Tuple<IDatumBuilder, IDatumBuilder>>.Empty),
                LazyThreadSafetyMode.ExecutionAndPublication
            );

            public static DictionaryBuilder Empty => empty.Value;

            public DictionaryBuilder Add(IDatumBuilder key, IDatumBuilder value)
            {
                return new DictionaryBuilder(values.Add(new Tuple<IDatumBuilder, IDatumBuilder>(key, value)));
            }

        }

        private static Datum BuildAll(this IDatumBuilder builder)
        {
            ImmutableHashSet<int> boxReferences = builder.BoxReferences;
            var boxes = ImmutableDictionary<int, MutableBoxDatum>.Empty;
            foreach(int boxReference in boxReferences)
            {
                boxes = boxes.Add(boxReference, new MutableBoxDatum(NullDatum.Value));
            }
            Datum result = builder.Build(boxes);
            var boxValues = builder.BoxValues;
            foreach(int boxReference in boxReferences)
            {
                if (boxValues.ContainsKey(boxReference))
                {
                    ImmutableList<IDatumBuilder> builders = boxValues[boxReference];
                    if (builders.Count == 1)
                    {
                        boxes[boxReference].Content = boxValues[boxReference][0].Build(boxes);
                    }
                    else throw new Exception($"Box {boxReference} has {builders.Count} values; expected 1");
                }
                else throw new Exception($"Box {boxReference} does not have a value");
            }
            return result;
        }

        private static ICharParser<IDatumBuilder> BuildParseMutableBox(ICharParser<IDatumBuilder> parseItem)
        {
            return ParseConvert<ImmutableList<object>, IDatumBuilder>
            (
                ParseSequence
                (
                    ParseExact("#b", StringComparison.InvariantCulture).ResultToObject(),
                    ParseOptRep
                    (
                        ParseConvert
                        (
                            ParseSequence
                            (
                                ParseExact("[", StringComparison.InvariantCulture).ResultToObject(),
                                ParseOptionalWhiteSpace.ResultToObject(),
                                ParseBigInteger.ResultToObject(),
                                ParseOptionalWhiteSpace.ResultToObject(),
                                ParseExact("]", StringComparison.InvariantCulture).ResultToObject()
                            ),
                            list => list[2],
                            null
                        ),
                        true,
                        false
                    )
                    .ResultToObject(),
                    ParseOptRep
                    (
                        ParseConvert
                        (
                            ParseSequence
                            (
                                ParseExact("=", StringComparison.InvariantCulture).ResultToObject(),
                                ParseOptionalWhiteSpace.ResultToObject(),
                                parseItem.ResultToObject()
                            ),
                            list => list[2],
                            null
                        ),
                        true,
                        false
                    )
                    .ResultToObject()
                ),
                list =>
                {
                    ImmutableList<object> keyList = (ImmutableList<object>)list[1];
                    ImmutableList<object> valueList = (ImmutableList<object>)list[2];
                    if (keyList.Count == 1)
                    {
                        BigInteger keyBig = (BigInteger)(keyList[0]);
                        if (keyBig < (BigInteger)int.MinValue || keyBig > (BigInteger)int.MaxValue)
                        {
                            throw new FormatException("Mutable box key out of range");
                        }
                        int key = (int)keyBig;
                        if (valueList.Count >= 1)
                        {
                            System.Diagnostics.Debug.Assert(valueList.Count == 1);

                            IDatumBuilder value = (IDatumBuilder)valueList[0];
                            return new BoxBuilder(key, Option<IDatumBuilder>.Some(value));
                        }
                        else
                        {
                            return new BoxBuilder(key, Option<IDatumBuilder>.None);
                        }
                    }
                    else
                    {
                        if (valueList.Count >= 1)
                        {
                            System.Diagnostics.Debug.Assert(valueList.Count == 1);

                            IDatumBuilder value = (IDatumBuilder)valueList[0];
                            return new BoxBuilder(null, Option<IDatumBuilder>.Some(value));
                        }
                        else
                        {
                            throw new FormatException("Mutable box requires key, value, or both to be specified");
                        }
                    }
                },
                null
            );
        }

        private static ICharParser<IDatumBuilder> Atom(this ICharParser<Datum> p)
        {
            return ParseConvert
            (
                p,
                dat => (IDatumBuilder)(new AtomBuilder(dat)),
                null
            );
        }

        private static ICharParser<IDatumBuilder> BuildQuoteLikeParser(ICharParser<IDatumBuilder> item, string token, string quoteSymbolName)
        {
            return ParseConvert
            (
                ParseSequence
                (
                    Token(token),
                    ParseConvert(item, d => (object)d, null)
                ),
                list =>
                {
                    return (IDatumBuilder)(new ListBuilder(ImmutableList<IDatumBuilder>.Empty.Add(new AtomBuilder(new SymbolDatum(quoteSymbolName))).Add((IDatumBuilder)list[1])));
                },
                "Failed to convert " + quoteSymbolName
            );
        }

        private static Lazy<ICharParser<Datum>> parseDatumWithBoxes = new Lazy<ICharParser<Datum>>(BuildParseDatumWithBoxes, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Datum> BuildParseDatumWithBoxes()
        {
            ICharParser<IDatumBuilder> parseDatum = GetParseVariable<IDatumBuilder>();

            ICharParser<IDatumBuilder> p0 = ParseAlternatives
            (
                ParseNull.Atom(),
                ParseFalse.Atom(),
                ParseTrue.Atom(),
                ParseConvert(ParseString, s => (Datum)(new StringDatum(s)), null).Atom(),
                ParseConvert(ParseBigRational, r => (Datum)(new RationalDatum(r)), null).Atom(),
                ParseConvert(ParseBigInteger, b => (Datum)(new IntDatum(b)), null).Atom(),
                ParseConvert(ParseDouble, d => (Datum)(new FloatDatum(d)), null).Atom(),
                ParseConvert(ParseSymbol, s => (Datum)(new SymbolDatum(s)), null).Atom(),
                ParseConvert(ParseChar, c => (Datum)(new CharDatum(c)), null).Atom(),
                ParseConvert(ParseGuid, g => (Datum)(new GuidDatum(g)), null).Atom(),
                ParseConvert(ParseByteArray, b => (Datum)(new ByteArrayDatum(b)), null).Atom(),
                BuildQuoteLikeParser(parseDatum, "'", "quote"),
                BuildQuoteLikeParser(parseDatum, "`", "quasiquote"),
                BuildQuoteLikeParser(parseDatum, ",@", "unquote-splicing"),
                BuildQuoteLikeParser(parseDatum, ",", "unquote"),
                ParseConvert(BuildListParser(parseDatum), lst => (IDatumBuilder)(new ListBuilder(lst)), null),
                ParseConvert(BuildSetParser(parseDatum, SetBuilder.Empty, (s, i) => s.Add(i)), s => (IDatumBuilder)s, null),
                ParseConvert(BuildDictionaryParser(parseDatum, parseDatum, DictionaryBuilder.Empty, (d, k, v) => d.Add(k, v)), dict => (IDatumBuilder)dict, null),
                BuildParseMutableBox(parseDatum).WithOptionalLeadingWhiteSpace()
            )
            .WithOptionalLeadingWhiteSpace();

            SetParseVariable(parseDatum, p0);

            return ParseConvert
            (
                p0,
                builder => builder.BuildAll(),
                null
            );
        }

        public static ICharParser<Datum> ParseDatumWithBoxes => parseDatumWithBoxes.Value;

    }
}
