using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Immutable;

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

        public static ICharParser<T> WithOptionalLeadingWhiteSpace<T>(this ICharParser<T> parser)
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

        public static ICharParser<object> ResultToObject<T>(this ICharParser<T> parser)
        {
            return CharParserBuilder.ParseConvert(parser, x => (object)x, null);
        }

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

        public static ICharParser<ImmutableList<T>> BuildListParser<T>(ICharParser<T> itemParser)
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseSequence
                (
                    new ICharParser<object>[]
                    {
                        CharParserBuilder.ParseExact("(", StringComparison.InvariantCulture).WithOptionalLeadingWhiteSpace().ResultToObject(),
                        CharParserBuilder.ParseOptRep(itemParser.WithOptionalLeadingWhiteSpace(), true, true).ResultToObject(),
                        CharParserBuilder.ParseExact(")", StringComparison.InvariantCulture).WithOptionalLeadingWhiteSpace().ResultToObject(),
                    }
                    .ToImmutableList()
                ),
                objs => (ImmutableList<T>)(objs[1]),
                null
            );
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
                    CharParserBuilder.ParseConvert(BuildListParser(parseDatum), lst => (Datum)(new ListDatum(lst)), null),
                }
                .ToImmutableList()
            );

            CharParserBuilder.SetParseVariable(parseDatum, p0);

            return p0;
        }

        public static ICharParser<Datum> ParseDatum => parseDatum.Value;
    }
}
