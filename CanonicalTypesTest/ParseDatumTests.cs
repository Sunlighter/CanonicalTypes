using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CanonicalTypes.Parsing;
using CanonicalTypes;
using System.Collections.Immutable;

namespace CanonicalTypesTest
{
    [TestClass]
    public class ParseDatumTests
    {
        private ICharParser<Datum> parseNull;
        private ICharParser<Datum> parseDatum;
        private Func<ParseResult<bool>, string> formatBoolResult;

        public ParseDatumTests()
        {
            parseNull = Parser.ParseNull;
            parseDatum = Parser.ParseDatum;
            formatBoolResult = Utility.GetParseResultStringConverter<bool>(b => b.ToString());
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
                    BooleanDatum.False,
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
                " ( #t #nil #f\r\n (#t #f) )"
            );

            Assert.AreEqual("{ success, pos = 0, len = 25, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseComplexDictionary()
        {
            Datum complexDictionary = DictionaryDatum.FromEnumerable
            (
                new Tuple<Datum, Datum>[]
                {
                    new Tuple<Datum, Datum>(BooleanDatum.True, BooleanDatum.False),
                    new Tuple<Datum, Datum>(NullDatum.Value, BooleanDatum.True),
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
                " { #t => #f, #nil => #t, (#t #f) => #nil }"
            );

            Assert.AreEqual("{ success, pos = 0, len = 42, value = True }", formatBoolResult(result));
        }
    }
}
