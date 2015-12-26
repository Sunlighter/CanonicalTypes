using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CanonicalTypes.Parsing
{
    public static partial class Parser
    {
        private static Lazy<ImmutableDictionary<string, string>> escapeChars = new Lazy<ImmutableDictionary<string, string>>(BuildEscapeChars, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ImmutableDictionary<string, string> BuildEscapeChars()
        {
            return ImmutableDictionary<string, string>.Empty
                .Add("\\", "\\")
                .Add("a", "\a")
                .Add("b", "\b")
                .Add("t", "\t")
                .Add("n", "\n")
                .Add("v", "\v")
                .Add("f", "\f")
                .Add("r", "\r");
        }

        private static Lazy<ICharParser<string>> hexEscape = new Lazy<ICharParser<string>>(BuildParseHexEscape, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<string> BuildParseHexEscape()
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseFromRegex
                (
                    new Regex
                    (
                        "\\G\\\\x([0-9A-Fa-f]{2})",
                        RegexOptions.Compiled
                    ),
                    "failed to parse hex escape"
                ),
                match =>
                {
                    return new string((char)(int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)), 1);
                },
                "failed to parse hex escape"
            );
        }

        private static Lazy<ICharParser<string>> unicodeEscape = new Lazy<ICharParser<string>>(BuildParseUnicodeEscape, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<string> BuildParseUnicodeEscape()
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseFromRegex
                (
                    new Regex
                    (
                        "\\G\\\\U([0-9A-Fa-f]{4})",
                        RegexOptions.Compiled
                    ),
                    "failed to parse Unicode escape"
                ),
                match =>
                {
                    return new string((char)(int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)), 1);
                },
                "failed to parse Unicode escape"
            );
        }

        private static ICharParser<string> BuildOneCharEscape(bool stringNotSymbol, ImmutableDictionary<string, string> escapes)
        {
            string failureMessage = "failed to parse single-char escape in " + (stringNotSymbol ? "string" : "symbol");

            return CharParserBuilder.ParseTryConvert
            (
                CharParserBuilder.ParseFromRegex
                (
                    new Regex
                    (
                        stringNotSymbol ? "\\G\\\\([\\\\\"abtnvfr])" : "\\G\\\\([\\\\|abtnvfr])",
                        RegexOptions.Compiled
                    ),
                    failureMessage
                ),
                match =>
                {
                    if (escapes.ContainsKey(match.Groups[1].Value))
                    {
                        return Option<string>.Some(escapes[match.Groups[1].Value]);
                    }
                    else return Option<string>.None;
                },
                failureMessage
            );
        }

        private static Lazy<ICharParser<string>> newlineEscape = new Lazy<ICharParser<string>>(BuildNewlineEscape, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<string> BuildNewlineEscape()
        {
            return BuildRegexToStringParser("\\G\\\\( |\\t)*(\\r(\\n?)|\\n(\\r?))", "Failed to parse newline escape");
        }

        public static ICharParser<string> NewlineEscape => newlineEscape.Value;

        private static ICharParser<string> BuildParseString(bool stringNotSymbol)
        {
            var localEscapeChars = escapeChars.Value;

            if (stringNotSymbol)
            {
                localEscapeChars = localEscapeChars.Add("\"", "\"");
            }
            else
            {
                localEscapeChars = localEscapeChars.Add("|", "|");
            }

            var stringChars = BuildRegexToStringParser
            (
                stringNotSymbol ? "\\G[^\\\\\"\\r\\n\\t]+" : "\\G[^\\\\|\\r\\n\\t]+",
                "Failed to parse " + (stringNotSymbol ? "string" : "symbol") + " chars"
            );

            var quote = stringNotSymbol ? Token("\"") : Token("|");

            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseSequence
                (
                    new ICharParser<object>[]
                    {
                        quote,
                        CharParserBuilder.ParseConvert
                        (
                            CharParserBuilder.ParseOptRep
                            (
                                CharParserBuilder.ParseAlternatives
                                (
                                    new ICharParser<string>[]
                                    {
                                        stringChars,
                                        BuildOneCharEscape(stringNotSymbol, localEscapeChars),
                                        CharParserBuilder.ParseConvert(NewlineEscape, str => string.Empty, "Failed to parse newline escape"),
                                        hexEscape.Value,
                                        unicodeEscape.Value
                                    }
                                    .ToImmutableList()
                                ),
                                true,
                                true
                            ),
                            list => (object)(string.Join(string.Empty, list)),
                            "Failed to parse " + (stringNotSymbol ? "string" : "symbol") + " parts"
                        ),
                        quote,
                    }
                    .ToImmutableList()
                ),
                listobj => (string)(listobj[1]),
                "Failed to parse " + (stringNotSymbol ? "string" : "symbol")
            );
        }

        private static Lazy<ICharParser<string>> parseString = new Lazy<ICharParser<string>>(() => BuildParseString(true), LazyThreadSafetyMode.ExecutionAndPublication);

        public static ICharParser<string> ParseString => parseString.Value;

        private static Lazy<ICharParser<string>> parseQuotedSymbol = new Lazy<ICharParser<string>>(() => BuildParseString(false), LazyThreadSafetyMode.ExecutionAndPublication);

        public static ICharParser<string> ParseQuotedSymbol => parseQuotedSymbol.Value;

        private static Lazy<ICharParser<string>> parseSymbol = new Lazy<ICharParser<string>>(BuildParseSymbol, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<string> BuildParseSymbol()
        {
            return CharParserBuilder.ParseAlternatives
            (
                new ICharParser<string>[]
                {
                    ParseUnquotedSymbol,
                    ParseQuotedSymbol
                }
                .ToImmutableList()
            );
        }

        public static ICharParser<string> ParseSymbol => parseSymbol.Value;
    }
}
