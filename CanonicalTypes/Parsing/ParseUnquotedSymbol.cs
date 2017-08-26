using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sunlighter.CanonicalTypes.Parsing
{
    public static partial class Parser
    {
        private static Lazy<ICharParser<string>> parseUnquotedSymbol = new Lazy<ICharParser<string>>(BuildParseUnquotedSymbol, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<string> BuildParseUnquotedSymbol()
        {
            var withoutMinus = BuildRegexToStringParser
            (
                "\\G[A-Za-z!$%&*+./:<=>?@^_~][A-Za-z0-9!$%&*+\\-./:<=>?@^_~]*",
                "Failed to parse unquoted symbol (without minus)"
            );

            var withMinus = BuildRegexToStringParser
            (
                "\\G-+(?:[A-Za-z!$%&*+./:<=>?@^_~][A-Za-z0-9!$%&*+\\-./:<=>?@^_~]*)?",
                "Failed to parse unquoted symbol (with minus)"
            );

            return ParseAlternatives
            (
                new ICharParser<string>[]
                {
                    withoutMinus,
                    withMinus
                }
                .ToImmutableList()
            );
        }

        public static ICharParser<string> ParseUnquotedSymbol => parseUnquotedSymbol.Value;

        private static Lazy<ICharParser<string>> parseUnquotedSymbolThenEof = new Lazy<ICharParser<string>>(BuildParseUnquotedSymbolThenEof, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<string> BuildParseUnquotedSymbolThenEof()
        {
            return ParseConvert
            (
                ParseSequence<object>
                (
                    ParseUnquotedSymbol.ResultToObject(),
                    ParseEOF.ResultToObject()
                ),
                lst => (string)(lst[0]),
                "Failed to parse unquoted symbol"
            );
        }

        public static ICharParser<string> ParseUnquotedSymbolThenEof => parseUnquotedSymbolThenEof.Value;
    }
}
