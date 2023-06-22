using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sunlighter.OptionLib;

namespace Sunlighter.CanonicalTypes.Parsing
{
    public static partial class Parser
    {
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

            return ParseTryConvert
            (
                ParseSequence
                (
                    intPart,
                    ParseAlternatives
                    (
                        ParseConvert
                        (
                            ParseSequence
                            (
                                fracPart,
                                exptPart
                            ),
                            list => string.Join(string.Empty, list),
                            "Failed to parse float (frac expt sequence)"
                        ),
                        fracPart,
                        exptPart
                    )
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
    }
}
