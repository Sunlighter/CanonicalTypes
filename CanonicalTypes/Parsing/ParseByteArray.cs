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
        public static byte[] Concat(this IEnumerable<byte[]> arrays)
        {
            int size = arrays.Select(a => a.Length).Aggregate(0, (s, t) => checked(s + t));
            byte[] result = new byte[size];
            int pos = 0;
            foreach(var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, pos, array.Length);
                pos += array.Length;
            }
            return result;
        }

        private static Lazy<ICharParser<ImmutableArray<byte>>> parseByteArray = new Lazy<ICharParser<ImmutableArray<byte>>>(BuildParseByteArray, LazyThreadSafetyMode.ExecutionAndPublication);

        public static byte[] HexToBytes(string hex, bool reverse)
        {
            int byteCount = hex.Length >> 1;
            byte[] result = new byte[byteCount];
            foreach (int i in Enumerable.Range(0, byteCount))
            {
                int j = reverse ? byteCount - i - 1 : i;
                result[j] = (byte)(int.Parse(hex.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber));
            }
            return result;
        }

        private static ICharParser<ImmutableArray<byte>> BuildParseByteArray()
        {
            return ParseConvert
            (
                ParseSequence
                (
                    Token("#y("),
                    ParseConvert
                    (
                        ParseOptRep
                        (
                            ParseAlternatives
                            (
                                ParseConvert
                                (
                                    ParseFromRegex
                                    (
                                        new Regex("\\G\\s+", RegexOptions.Compiled),
                                        "Expected white space"
                                    ),
                                    match => (object)DBNull.Value,
                                    "Expected white space"
                                ),
                                ParseConvert
                                (
                                    ParseFromRegex
                                    (
                                        new Regex("\\G([A-Fa-f0-9]{2})+", RegexOptions.Compiled | RegexOptions.ExplicitCapture),
                                        "Expected hex digits"
                                    ),
                                    match =>
                                    {
                                        return (object)HexToBytes(match.Value, false);
                                    },
                                    "Expected hex digits"
                                ),
                                ParseConvert
                                (
                                    ParseFromRegex
                                    (
                                        new Regex("\\G\\[\\s*(?<digits>([A-Fa-f0-9]{2}|\\s+)+)\\]", RegexOptions.Compiled | RegexOptions.ExplicitCapture),
                                        "Expected bracketed hex digits"
                                    ),
                                    match =>
                                    {
                                        return (object)HexToBytes(string.Join(string.Empty, Regex.Split(match.Groups["digits"].Value, "\\s+")), true);
                                    },
                                    "Expected bracketed hex digits"
                                )
                            ),
                            true,
                            true
                        ),
                        list => (object)(list.OfType<byte[]>().Concat()),
                        "failed to concat byte arrays"
                    ),
                    Token(")")
                ),
                seq => ImmutableArray<byte>.Empty.AddRange((byte[])seq[1]),
                "Failed to parse byte array"
            );
        }
        
        public static ICharParser<ImmutableArray<byte>> ParseByteArray => parseByteArray.Value;
    }
}
