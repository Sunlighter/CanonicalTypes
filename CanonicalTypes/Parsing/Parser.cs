using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace CanonicalTypes.Parsing
{
    public static class Parser
    {
        private static Lazy<ICharParser<Nothing>> optionalWhiteSpace = new Lazy<ICharParser<Nothing>>(BuildOptionalWhiteSpace, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Nothing> BuildOptionalWhiteSpace()
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseFromRegex
                (
                    new Regex("\\G\\s*", RegexOptions.Compiled),
                    "whitespace expected"
                ),
                m => Nothing.Value,
                null
            );
        }

        public static ICharParser<Nothing> ParseOptionalWhiteSpace => optionalWhiteSpace.Value;

        private static ICharParser<T> WithOptionalLeadingWhiteSpace_Internal<T>(this ICharParser<T> parser)
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseSequence
                (
                    new[]
                    {
                        CharParserBuilder.ParseConvert
                        (
                            ParseOptionalWhiteSpace,
                            _ => default(T),
                            null
                        ),
                        parser
                    }.ToImmutableList()
                ),
                lst => lst[1],
                null
            );
        }

        private static object olwsSyncRoot = new object();
        private static ConditionalWeakTable<object, object> olwsDict = new ConditionalWeakTable<object, object>();

        public static ICharParser<T> WithOptionalLeadingWhiteSpace<T>(this ICharParser<T> parser)
        {
            lock(olwsSyncRoot)
            {
                object result;
                bool hasResult = olwsDict.TryGetValue(parser, out result);
                if (hasResult)
                {
                    return (ICharParser<T>)result;
                }
                else
                {
                    result = WithOptionalLeadingWhiteSpace_Internal<T>(parser);
                    olwsDict.Add(parser, result);
                    olwsDict.Add(result, result);
                    return (ICharParser<T>)result;
                }
            }
        }

        public static ICharParser<object> ResultToObject<T>(this ICharParser<T> parser)
        {
            return CharParserBuilder.ParseConvert(parser, x => (object)x, null);
        }

        #region ParseNull

        private static Lazy<ICharParser<Datum>> parseNull = new Lazy<ICharParser<Datum>>(BuildParseNull, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Datum> BuildParseNull()
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseExact
                (
                    "#nil",
                    StringComparison.InvariantCulture
                ),
                _ => (Datum)NullDatum.Value,
                null
            ).WithOptionalLeadingWhiteSpace();
        }

        public static ICharParser<Datum> ParseNull => parseNull.Value;

        #endregion

        #region ParseFalse

        private static Lazy<ICharParser<Datum>> parseFalse = new Lazy<ICharParser<Datum>>(BuildParseFalse, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Datum> BuildParseFalse()
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseExact
                (
                    "#f",
                    StringComparison.InvariantCulture
                ),
                _ => (Datum)(BooleanDatum.False),
                null
            ).WithOptionalLeadingWhiteSpace();
        }

        public static ICharParser<Datum> ParseFalse => parseFalse.Value;

        #endregion

        #region ParseTrue

        private static Lazy<ICharParser<Datum>> parseTrue = new Lazy<ICharParser<Datum>>(BuildParseTrue, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Datum> BuildParseTrue()
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseExact
                (
                    "#t",
                    StringComparison.InvariantCulture
                ),
                _ => (Datum)(BooleanDatum.True),
                null
            ).WithOptionalLeadingWhiteSpace();
        }

        public static ICharParser<Datum> ParseTrue => parseTrue.Value;

        #endregion

        private static ICharParser<object> Token(string tokenStr)
        {
            return CharParserBuilder.ParseExact(tokenStr, StringComparison.InvariantCulture).WithOptionalLeadingWhiteSpace().ResultToObject();
        }

        public static ICharParser<ImmutableList<T>> BuildListParser<T>(ICharParser<T> itemParser)
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseSequence
                (
                    new ICharParser<object>[]
                    {
                        Token("("),
                        CharParserBuilder.ParseOptRep(itemParser.WithOptionalLeadingWhiteSpace(), true, true).ResultToObject(),
                        Token(")"),
                    }
                    .ToImmutableList()
                ),
                objs => (ImmutableList<T>)(objs[1]),
                null
            );
        }

        public static ICharParser<TDict> BuildDictionaryParser<TKey, TValue, TDict>
        (
            ICharParser<TKey> keyParser,
            ICharParser<TValue> valueParser,
            TDict empty,
            Func<TDict, TKey, TValue, TDict> addItem
        )
        {
            ICharParser<Tuple<TKey, TValue>> kvp = CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseSequence
                (
                    new ICharParser<object>[]
                    {
                        keyParser.ResultToObject(),
                        Token("=>"),
                        valueParser.ResultToObject(),
                    }
                    .ToImmutableList()
                ),
                objs => new Tuple<TKey, TValue>((TKey)objs[0], (TValue)objs[2]),
                null
            );

            var dict = CharParserBuilder.ParseSequence
            (
                new ICharParser<object>[]
                {
                    Token("{"),
                    CharParserBuilder.ParseOptRep
                    (
                        CharParserBuilder.ParseConvert
                        (
                            CharParserBuilder.ParseSequence
                            (
                                new ICharParser<object>[]
                                {
                                    kvp.ResultToObject(),
                                    Token(",")
                                }
                                .ToImmutableList()
                            ),
                            lst => (Tuple<TKey, TValue>)lst[0],
                            null
                        ),
                        true,
                        true
                    )
                    .ResultToObject(),
                    CharParserBuilder.ParseOptRep
                    (
                        kvp,
                        true,
                        false
                    )
                    .ResultToObject(),
                    Token("}"),
                }
                .ToImmutableList()
            );

            return CharParserBuilder.ParseConvert
            (
                dict,
                objs =>
                {
                    ImmutableList<Tuple<TKey, TValue>> l1 = (ImmutableList<Tuple<TKey, TValue>>)objs[1];
                    ImmutableList<Tuple<TKey, TValue>> l2 = (ImmutableList<Tuple<TKey, TValue>>)objs[2];

                    TDict v = empty;
                    foreach(Tuple<TKey, TValue> kvp0 in l1.Concat(l2))
                    {
                        v = addItem(v, kvp0.Item1, kvp0.Item2);
                    }
                    return v;
                },
                null
            );
        }

        private static Lazy<ICharParser<BigInteger>> parseBigInteger = new Lazy<ICharParser<BigInteger>>(BuildParseBigInteger, LazyThreadSafetyMode.ExecutionAndPublication);

        public static ICharParser<BigInteger> ParseBigInteger => parseBigInteger.Value;

        private static ICharParser<BigInteger> BuildParseBigInteger()
        {
            return CharParserBuilder.ParseTryConvert
            (
                CharParserBuilder.ParseFromRegex
                (
                    new Regex
                    (
                        "\\G-?(?:0(?![0-9])|(?:[1-9][0-9]*))(?![/\\.eE])",
                        RegexOptions.Compiled | RegexOptions.ExplicitCapture
                    ),
                    "Failed to parse integer"
                ),
                match =>
                {
                    BigInteger b;
                    if (BigInteger.TryParse(match.Value, out b))
                    {
                        return Option<BigInteger>.Some(b);
                    }
                    else return Option<BigInteger>.None;
                },
                "Failed to parse integer"
            ).WithOptionalLeadingWhiteSpace();
        }

        private static Lazy<ICharParser<Datum>> parseDatum = new Lazy<ICharParser<Datum>>(BuildParseDatum, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Datum> BuildParseDatum()
        {
            ICharParser<Datum> parseDatum = CharParserBuilder.GetParseVariable<Datum>();

            ICharParser<Datum> p0 = CharParserBuilder.ParseAlternatives
            (
                new[]
                {
                    ParseNull,
                    ParseFalse,
                    ParseTrue,
                    CharParserBuilder.ParseConvert(ParseBigInteger, b => (Datum)(new IntDatum(b)), null),
                    CharParserBuilder.ParseConvert(BuildListParser(parseDatum), lst => (Datum)(new ListDatum(lst)), null),
                    CharParserBuilder.ParseConvert(BuildDictionaryParser(parseDatum, parseDatum, DictionaryDatum.Empty, (d, k, v) => d.Add(k, v)), dict => (Datum)dict, null),
                }
                .ToImmutableList()
            );

            CharParserBuilder.SetParseVariable(parseDatum, p0);

            return p0;
        }

        public static ICharParser<Datum> ParseDatum => parseDatum.Value;
    }
}
