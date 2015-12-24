using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CanonicalTypes.Parsing
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

            return CharParserBuilder.ParseAlternatives
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
    }
}
