using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CanonicalTypes;
using CanonicalTypes.Parsing;

namespace CanonicalTypesTest
{
    [TestClass]
    public class ParseOptionalWsTest
    {
        private ICharParser<Nothing> ws;
        private Func<ParseResult<Nothing>, string> toString;

        public ParseOptionalWsTest()
        {
            ws = Parser.ParseOptionalWhiteSpace;
            toString = Utility.GetParseResultStringConverter<Nothing>(n => "null");
        }

        [TestMethod]
        public void TestEmptyString()
        {
            Assert.AreEqual("{ success, pos = 0, len = 0, value = null }", toString(CharParserContext.TryParse(ws, "")));
        }

        [TestMethod]
        public void TestLeadingSpaces()
        {
            Assert.AreEqual("{ success, pos = 0, len = 2, value = null }", toString(CharParserContext.TryParse(ws, "  a")));
        }

        [TestMethod]
        public void TestNoSpace()
        {
            Assert.AreEqual("{ success, pos = 0, len = 0, value = null }", toString(CharParserContext.TryParse(ws, "a")));
        }
    }
}
