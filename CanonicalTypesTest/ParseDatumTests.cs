using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CanonicalTypes.Parsing;
using CanonicalTypes;
using System.Collections.Immutable;
using System.Numerics;

namespace CanonicalTypesTest
{
    [TestClass]
    public class ParseDatumTests
    {
        private ICharParser<Datum> parseNull;
        private ICharParser<Datum> parseDatum;
        private Func<ParseResult<bool>, string> formatBoolResult;
        private Func<ParseResult<BigInteger>, string> formatBigIntegerResult;
        private Func<ParseResult<double>, string> formatDoubleResult;

        public ParseDatumTests()
        {
            parseNull = Parser.ParseNull;
            parseDatum = Parser.ParseDatum;
            formatBoolResult = Utility.GetParseResultStringConverter<bool>(b => b.ToString());
            formatBigIntegerResult = Utility.GetParseResultStringConverter<BigInteger>(b => b.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            formatDoubleResult = Utility.GetParseResultStringConverter<double>(d => d.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        [TestMethod]
        public void ParseNull()
        {
            var result = CharParserContext.TryParse
            (
                CharParserBuilder.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, NullDatum.Value),
                    null
                ),
                "#nil"
            );

            Assert.AreEqual("{ success, pos = 0, len = 4, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseSpaceNull()
        {
            var result = CharParserContext.TryParse
            (
                CharParserBuilder.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, NullDatum.Value),
                    null
                ),
                " #nil"
            );

            Assert.AreEqual("{ success, pos = 0, len = 5, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseSpaceTrue()
        {
            var result = CharParserContext.TryParse
            (
                CharParserBuilder.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, BooleanDatum.True),
                    null
                ),
                " #t"
            );

            Assert.AreEqual("{ success, pos = 0, len = 3, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseComplexList()
        {
            Datum complexList = new ListDatum
            (
                new Datum[]
                {
                    BooleanDatum.True,
                    NullDatum.Value,
                    new FloatDatum(-3.75),
                    new ListDatum
                    (
                        new Datum[]
                        {
                            BooleanDatum.True,
                            BooleanDatum.False,
                        }
                        .ToImmutableList()
                    )
                }
                .ToImmutableList()
            );

            var result = CharParserContext.TryParse
            (
                CharParserBuilder.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, complexList),
                    null
                ),
                " ( #t #nil -3.75\r\n (#t #f) )"
            );

            Assert.AreEqual("{ success, pos = 0, len = 28, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseComplexDictionary()
        {
            Datum complexDictionary = DictionaryDatum.FromEnumerable
            (
                new Tuple<Datum, Datum>[]
                {
                    new Tuple<Datum, Datum>(BooleanDatum.True, BooleanDatum.False),
                    new Tuple<Datum, Datum>(NullDatum.Value, new IntDatum(1000)),
                    new Tuple<Datum, Datum>(new ListDatum(new Datum[] { BooleanDatum.True, BooleanDatum.False }.ToImmutableList()), NullDatum.Value),
                }
            );

            var result = CharParserContext.TryParse
            (
                CharParserBuilder.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, complexDictionary),
                    null
                ),
                " { #t => #f, #nil => 1000, (#t #f) => #nil }"
            );

            Assert.AreEqual("{ success, pos = 0, len = 44, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseBigInteger()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseBigInteger,
                "-2359862495629582635"
            );

            Assert.AreEqual("{ success, pos = 0, len = 20, value = -2359862495629582635 }", formatBigIntegerResult(result));
        }

        [TestMethod]
        public void ParseNotBigIntegerButRational()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseBigInteger,
                "1/3"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse integer\" } }", formatBigIntegerResult(result));
        }

        [TestMethod]
        public void ParseNotBigIntegerButFloatE()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseBigInteger,
                "1e+10"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse integer\" } }", formatBigIntegerResult(result));
        }

        [TestMethod]
        public void ParseNotBigIntegerButFloat()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseBigInteger,
                "1.5"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse integer\" } }", formatBigIntegerResult(result));
        }

        [TestMethod]
        public void ParseDouble()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseDouble,
                "6.125"
            );

            Assert.AreEqual("{ success, pos = 0, len = 5, value = 6.125 }", formatDoubleResult(result));
        }

        [TestMethod]
        public void ParseNotDoubleButRational()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseDouble,
                "1/3"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse float (int part)\" } }", formatDoubleResult(result));
        }

        [TestMethod]
        public void ParseNotDoubleButInteger()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseDouble,
                "10000000"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse float (int part)\" } }", formatDoubleResult(result));
        }

        [TestMethod]
        public void ParseDoubleWithExponent()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseDouble,
                "1.5e+24"
            );

            Assert.AreEqual("{ success, pos = 0, len = 7, value = 1.5E+24 }", formatDoubleResult(result));
        }
    }
}
