using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Sunlighter.OptionLib;

namespace Sunlighter.CanonicalTypes.Parsing
{
    public static partial class Parser
    {
        private static Lazy<ICharParser<BigInteger>> parseBigInteger = new Lazy<ICharParser<BigInteger>>(BuildParseBigInteger, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<BigInteger> BuildParseBigInteger()
        {
            return ParseTryConvert
            (
                ParseFromRegex
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
    }
}
