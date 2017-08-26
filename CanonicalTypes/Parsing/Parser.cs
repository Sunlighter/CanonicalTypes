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

namespace Sunlighter.CanonicalTypes.Parsing
{
    public static partial class Parser
    {
        #region Optional White Space

        private static Lazy<ICharParser<Nothing>> optionalWhiteSpace = new Lazy<ICharParser<Nothing>>(BuildOptionalWhiteSpace, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Nothing> BuildOptionalWhiteSpace()
        {
            return ParseConvert
            (
                ParseFromRegex
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
            return ParseConvert
            (
                ParseSequence
                (
                    ParseConvert
                    (
                        ParseOptionalWhiteSpace,
                        _ => default(T),
                        null
                    ),
                    parser
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
            return ParseConvert(parser, x => (object)x, null);
        }

        #region ParseNull

        private static Lazy<ICharParser<Datum>> parseNull = new Lazy<ICharParser<Datum>>(BuildParseNull, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Datum> BuildParseNull()
        {
            return ParseConvert
            (
                ParseExact
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
            return ParseConvert
            (
                ParseExact
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
            return ParseConvert
            (
                ParseExact
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
            return ParseExact(tokenStr, StringComparison.InvariantCulture).WithOptionalLeadingWhiteSpace().ResultToObject();
        }

        public static ICharParser<ImmutableList<T>> BuildListParser<T>(ICharParser<T> itemParser)
        {
            return ParseConvert
            (
                ParseSequence
                (
                    Token("("),
                    ParseOptRep(itemParser.WithOptionalLeadingWhiteSpace(), true, true).ResultToObject(),
                    Token(")")
                ),
                objs => (ImmutableList<T>)(objs[1]),
                null
            );
        }

        public static ICharParser<string> BuildRegexToStringParser(string regexStr, string errorMessage)
        {
            return ParseConvert
            (
                ParseFromRegex
                (
                    new Regex(regexStr, RegexOptions.Compiled | RegexOptions.ExplicitCapture),
                    errorMessage
                ),
                match => match.Value,
                errorMessage
            );
        }

        public static ICharParser<Datum> BuildQuoteLikeParser(ICharParser<Datum> item, string token, string quoteSymbolName)
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
                    return (Datum)(new ListDatum(ImmutableList<Datum>.Empty.Add(new SymbolDatum(quoteSymbolName)).Add((Datum)list[1])));
                },
                "Failed to convert " + quoteSymbolName
            );
        }

        private static Lazy<ICharParser<Datum>> parseDatum = new Lazy<ICharParser<Datum>>(BuildParseDatum, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Datum> BuildParseDatum()
        {
            ICharParser<Datum> parseDatum = GetParseVariable<Datum>();

            ICharParser<Datum> p0 = ParseAlternatives
            (
                ParseNull,
                ParseFalse,
                ParseTrue,
                ParseConvert(ParseString, s => (Datum)(new StringDatum(s)), null),
                ParseConvert(ParseBigRational, r => (Datum)(new RationalDatum(r)), null),
                ParseConvert(ParseBigInteger, b => (Datum)(new IntDatum(b)), null),
                ParseConvert(ParseDouble, d => (Datum)(new FloatDatum(d)), null),
                ParseConvert(ParseSymbol, s => (Datum)(new SymbolDatum(s)), null),
                ParseConvert(ParseChar, c => (Datum)(new CharDatum(c)), null),
                ParseConvert(ParseGuid, g => (Datum)(new GuidDatum(g)), null),
                ParseConvert(ParseByteArray, b => (Datum)(new ByteArrayDatum(b)), null),
                BuildQuoteLikeParser(parseDatum, "'", "quote"),
                BuildQuoteLikeParser(parseDatum, "`", "quasiquote"),
                BuildQuoteLikeParser(parseDatum, ",@", "unquote-splicing"),
                BuildQuoteLikeParser(parseDatum, ",", "unquote"),
                ParseConvert(BuildListParser(parseDatum), lst => (Datum)(new ListDatum(lst)), null),
                ParseConvert(BuildSetParser(parseDatum, SetDatum.Empty, (s, i) => s.Add(i)), s => (Datum)s, null),
                ParseConvert(BuildDictionaryParser(parseDatum, parseDatum, DictionaryDatum.Empty, (d, k, v) => d.Add(k, v)), dict => (Datum)dict, null)
            )
            .WithOptionalLeadingWhiteSpace();

            SetParseVariable(parseDatum, p0);

            return p0;
        }

        public static ICharParser<Datum> ParseDatum => parseDatum.Value;
    }
}
