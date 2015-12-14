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

        private static Lazy<ICharParser<Datum>> parseNull = new Lazy<ICharParser<Datum>>(BuildParseNull, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Datum> BuildParseNull()
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseExact
                (
                    "#null",
                    StringComparison.InvariantCulture
                ),
                _ => (Datum)NullDatum.Value,
                "null conversion failed"
            ).WithOptionalLeadingWhiteSpace();
        }

        public static ICharParser<Datum> ParseNull => parseNull.Value;

        private static Lazy<ICharParser<Datum>> parseDatum = new Lazy<ICharParser<Datum>>(BuildParseDatum, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Datum> BuildParseDatum()
        {
            throw new NotImplementedException();
        }

        public static ICharParser<Datum> ParseDatum => parseDatum.Value;
    }
}
