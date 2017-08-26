using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sunlighter.CanonicalTypes.Parsing
{
    public static partial class Parser
    {
        private static Lazy<ICharParser<Guid>> parseGuid = new Lazy<ICharParser<Guid>>(BuildParseGuid, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ICharParser<Guid> BuildParseGuid()
        {
            return ParseTryConvert
            (
                ParseFromRegex
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
    }
}
