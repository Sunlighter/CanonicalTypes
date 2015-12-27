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
        private static Lazy<ImmutableDictionary<string, char>> characterNames = new Lazy<ImmutableDictionary<string, char>>(BuildCharacterNames, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ImmutableDictionary<string, char> BuildCharacterNames()
        {
            return ImmutableDictionary<string, char>.Empty
                .Add("nul", (char)0)
                .Add("bel", '\a')
                .Add("backspace", '\b')
                .Add("tab", '\t')
                .Add("newline", '\n')
                .Add("vt", '\v')
                .Add("page", '\f')
                .Add("return", '\r')
                .Add("space", ' ');
        }

        public static ImmutableDictionary<string, char> CharacterNames => characterNames.Value;

        private static Lazy<ICharParser<char>> parseNamedChar = new Lazy<ICharParser<char>>(BuildParseNamedChar, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<char> BuildParseNamedChar()
        {
            return ParseTryConvert
            (
                ParseFromRegex
                (
                    new Regex
                    (
                        "\\G#\\\\(?<name>[A-Za-z][A-Za-z0-9]*)",
                        RegexOptions.Compiled | RegexOptions.ExplicitCapture
                    ),
                    "Failed to parse named character"
                ),
                match =>
                {
                    string name = match.Groups["name"].Value;
                    if (CharacterNames.ContainsKey(name))
                    {
                        return Option<char>.Some(CharacterNames[name]);
                    }
                    else
                    {
                        return Option<char>.None;
                    }
                },
                "Failed to parse named character"
            );
        }

        public static ICharParser<char> ParseNamedChar => parseNamedChar.Value;

        private static Lazy<ICharParser<char>> parseLiteralChar = new Lazy<ICharParser<char>>(BuildParseLiteralChar, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<char> BuildParseLiteralChar()
        {
            return ParseTryConvert
            (
                ParseFromRegex
                (
                    new Regex
                    (
                        "\\G#\\\\(?<ch>\\p{L}|\\p{M}|\\p{N}|\\p{P}|\\p{S})(?![A-Za-z0-9])",
                        RegexOptions.Compiled | RegexOptions.ExplicitCapture
                    ),
                    "Failed to parse literal character"
                ),
                match =>
                {
                    string ch = match.Groups["ch"].Value;
                    if (ch.Length == 1)
                    {
                        return Option<char>.Some(ch[0]);
                    }
                    else
                    {
                        // in the future it might be cool to add support for surrogate pairs as a single char

                        return Option<char>.None;
                    }
                },
                "Failed to parse literal character"
            );
        }

        public static ICharParser<char> ParseLiteralChar => parseLiteralChar.Value;

        private static Lazy<ICharParser<char>> parseHexChar = new Lazy<ICharParser<char>>(BuildParseHexChar, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<char> BuildParseHexChar()
        {
            return ParseTryConvert
            (
                ParseFromRegex
                (
                    new Regex
                    (
                        "\\G#\\\\x(?<hex>[0-9A-Fa-f]+)(?![A-Za-z0-9])",
                        RegexOptions.Compiled | RegexOptions.ExplicitCapture
                    ),
                    "Failed to parse hex character"
                ),
                match =>
                {
                    string hex = match.Groups["hex"].Value;
                    int hexValue;
                    if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo, out hexValue))
                    {
                        return Option<char>.Some((char)hexValue);
                    }
                    else
                    {
                        return Option<char>.None;
                    }
                },
                "Failed to parse hex character"
            );
        }

        public static ICharParser<char> ParseHexChar => parseHexChar.Value;

        private static Lazy<ICharParser<char>> parseChar = new Lazy<ICharParser<char>>(BuildParseChar, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<char> BuildParseChar()
        {
            return ParseAlternatives
            (
                new ICharParser<char>[]
                {
                    ParseNamedChar,
                    ParseHexChar,
                    ParseLiteralChar,
                }
                .ToImmutableList()
            );
        }

        public static ICharParser<char> ParseChar => parseChar.Value;
    }
}
