using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CanonicalTypes.Parsing
{
    public static partial class Parser
    {
        private static Lazy<ICharParser<BigRational>> parseBigRational = new Lazy<ICharParser<BigRational>>(BuildParseBigRational, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<BigRational> BuildParseBigRational()
        {
            return ParseTryConvert
           (
               ParseFromRegex
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
    }
}
