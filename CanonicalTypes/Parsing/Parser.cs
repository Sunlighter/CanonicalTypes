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
        #region Optional White Space

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

        #endregion

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

        // olws stands for Optional Leading White Space.

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
            );
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
            );
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
            );
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

        public static ICharParser<TSet> BuildSetParser<TItem, TSet>
        (
            ICharParser<TItem> itemParser,
            TSet empty,
            Func<TSet, TItem, TSet> addItem
        )
        {
            var set = CharParserBuilder.ParseSequence
            (
                new ICharParser<object>[]
                {
                    Token("#s{"),
                    CharParserBuilder.ParseOptRep
                    (
                        itemParser, true, true

                    ).ResultToObject(),
                    Token("}")
                }
                .ToImmutableList()
            );

            return CharParserBuilder.ParseConvert
            (
                set,
                objs =>
                {
                    ImmutableList<TItem> items = (ImmutableList<TItem>)objs[1];

                    TSet s = empty;
                    foreach (TItem item in items)
                    {
                        s = addItem(s, item);
                    }

                    return s;
                },
                null
            );
        }

        #region Parse BigInteger (Decimal)

        private static Lazy<ICharParser<BigInteger>> parseBigInteger = new Lazy<ICharParser<BigInteger>>(BuildParseBigInteger, LazyThreadSafetyMode.ExecutionAndPublication);

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
            );
        }

        public static ICharParser<BigInteger> ParseBigInteger => parseBigInteger.Value;

        #endregion

        #region Parse Guid

        private static Lazy<ICharParser<Guid>> parseGuid = new Lazy<ICharParser<Guid>>(BuildParseGuid, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Guid> BuildParseGuid()
        {
            return CharParserBuilder.ParseTryConvert
            (
                CharParserBuilder.ParseFromRegex
                (
                    new Regex
                    (
                        "\\G#g\\{(?<digits>[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12})\\}",
                        RegexOptions.Compiled | RegexOptions.ExplicitCapture
                    ),
                    "Failed to parse guid"
                ),
                match =>
                {
                    Guid g;
                    if (Guid.TryParse(match.Groups["digits"].Value, out g))
                    {
                        return Option<Guid>.Some(g);
                    }
                    else return Option<Guid>.None;
                },
                "Failed to parse guid"
            );
        }

        public static ICharParser<Guid> ParseGuid => parseGuid.Value;

        #endregion

        #region Parse BigRational (Decimal)

        private static Lazy<ICharParser<BigRational>> parseBigRational = new Lazy<ICharParser<BigRational>>(BuildParseBigRational, LazyThreadSafetyMode.ExecutionAndPublication);
        
        private static ICharParser<BigRational> BuildParseBigRational()
        {
             return CharParserBuilder.ParseTryConvert
            (
                CharParserBuilder.ParseFromRegex
                (
                    new Regex
                    (
                        "\\G(?<n>-?(?:0(?![0-9])|(?:[1-9][0-9]*)))/(?<d>[1-9][0-9]*)",
                        RegexOptions.Compiled | RegexOptions.ExplicitCapture
                    ),
                    "Failed to parse rational"
                ),
                match =>
                {
                    BigInteger n;
                    BigInteger d;
                    if (BigInteger.TryParse(match.Groups["n"].Value, out n) && BigInteger.TryParse(match.Groups["d"].Value, out d))
                    {
                        return Option<BigRational>.Some(new BigRational(n, d));
                    }
                    else return Option<BigRational>.None;
                },
                "Failed to parse rational"
            );
        } 
        
        public static ICharParser<BigRational> ParseBigRational => parseBigRational.Value;
         
        #endregion

        public static ICharParser<string> BuildRegexToStringParser(string regexStr, string errorMessage)
        {
            return CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseFromRegex
                (
                    new Regex(regexStr, RegexOptions.Compiled | RegexOptions.ExplicitCapture),
                    errorMessage
                ),
                match => match.Value,
                errorMessage
            );
        }

        #region Parse Double (Decimal)

        private static Lazy<ICharParser<double>> parseDouble = new Lazy<ICharParser<double>>(BuildParseDouble, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<double> BuildParseDouble()
        {
            var intPart = BuildRegexToStringParser
            (
                "\\G-?(?:0(?![0-9])|(?:[1-9][0-9]*))(?=[\\.eE])",
                "Failed to parse float (int part)"
            );

            var fracPart = BuildRegexToStringParser
            (
                "\\G\\.[0-9]*",
                "Failed to parse float (frac part)"
            );

            var exptPart = BuildRegexToStringParser
            (
                "\\G[Ee](\\+|-)?[1-9][0-9]*",
                "Failed to parse float (expt part)"
            );

            return CharParserBuilder.ParseTryConvert
            (
                CharParserBuilder.ParseSequence
                (
                    new ICharParser<string>[]
                    {
                        intPart,
                        CharParserBuilder.ParseAlternatives
                        (
                            new ICharParser<string>[]
                            {
                                CharParserBuilder.ParseConvert
                                (
                                    CharParserBuilder.ParseSequence
                                    (
                                        new ICharParser<string>[]
                                        {
                                            fracPart,
                                            exptPart,
                                        }
                                        .ToImmutableList()
                                    ),
                                    list => string.Join(string.Empty, list),
                                    "Failed to parse float (frac expt sequence)"
                                ),
                                fracPart,
                                exptPart,
                            }
                            .ToImmutableList()
                        )
                    }
                    .ToImmutableList()

                ),
                list2 =>
                {
                    string str = string.Join(string.Empty, list2);
                    double val;
                    if (double.TryParse(str, out val))
                    {
                        return Option<double>.Some(val);
                    }
                    else return Option<double>.None;
                },
                "Failed to parse float (conversion)"
            );
        }

        public static ICharParser<double> ParseDouble => parseDouble.Value;

        #endregion

        #region Parse Unquoted Symbol

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

        #endregion

        #region Parse Quoted String / Symbol

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

        #endregion

        #region Parse Character

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
            return CharParserBuilder.ParseTryConvert
            (
                CharParserBuilder.ParseFromRegex
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
            return CharParserBuilder.ParseTryConvert
            (
                CharParserBuilder.ParseFromRegex
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

        public static ICharParser<char> ParseLiteralChar = parseLiteralChar.Value;

        private static Lazy<ICharParser<char>> parseHexChar = new Lazy<ICharParser<char>>(BuildParseHexChar, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<char> BuildParseHexChar()
        {
            return CharParserBuilder.ParseTryConvert
            (
                CharParserBuilder.ParseFromRegex
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

        public static ICharParser<char> ParseHexChar = parseHexChar.Value;

        private static Lazy<ICharParser<char>> parseChar = new Lazy<ICharParser<char>>(BuildParseChar, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<char> BuildParseChar()
        {
            return CharParserBuilder.ParseAlternatives
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

        #endregion

        private static Lazy<ICharParser<Datum>> parseDatum = new Lazy<ICharParser<Datum>>(BuildParseDatum, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Datum> BuildParseDatum()
        {
            ICharParser<Datum> parseDatum = CharParserBuilder.GetParseVariable<Datum>();

            ICharParser<Datum> p0 = CharParserBuilder.ParseAlternatives
            (
                new ICharParser<Datum>[]
                {
                    ParseNull,
                    ParseFalse,
                    ParseTrue,
                    CharParserBuilder.ParseConvert(ParseString, s => (Datum)(new StringDatum(s)), null),
                    CharParserBuilder.ParseConvert(ParseBigRational, r => (Datum)(new RationalDatum(r)), null),
                    CharParserBuilder.ParseConvert(ParseBigInteger, b => (Datum)(new IntDatum(b)), null),
                    CharParserBuilder.ParseConvert(ParseDouble, d => (Datum)(new FloatDatum(d)), null),
                    CharParserBuilder.ParseConvert(ParseSymbol, s => (Datum)(new SymbolDatum(s)), null),
                    CharParserBuilder.ParseConvert(ParseChar, c => (Datum)(new CharDatum(c)), null),
                    CharParserBuilder.ParseConvert(ParseGuid, g => (Datum)(new GuidDatum(g)), null),
                    CharParserBuilder.ParseConvert(BuildListParser(parseDatum), lst => (Datum)(new ListDatum(lst)), null),
                    CharParserBuilder.ParseConvert(BuildSetParser(parseDatum, SetDatum.Empty, (s, i) => s.Add(i)), s => (Datum)s, null),
                    CharParserBuilder.ParseConvert(BuildDictionaryParser(parseDatum, parseDatum, DictionaryDatum.Empty, (d, k, v) => d.Add(k, v)), dict => (Datum)dict, null),
                }
                .ToImmutableList()
            )
            .WithOptionalLeadingWhiteSpace();

            // TODO: bytearray, mutablebox

            CharParserBuilder.SetParseVariable(parseDatum, p0);

            return p0;
        }

        public static ICharParser<Datum> ParseDatum => parseDatum.Value;
    }
}
