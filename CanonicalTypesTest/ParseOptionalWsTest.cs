using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sunlighter.CanonicalTypes;
using Sunlighter.CanonicalTypes.Parsing;

namespace CanonicalTypesTest
{
    [TestClass]
    public class ParseOptionalWsTest
    {
        private ICharParser<Nothing> ws;
        private ICharParser<string> a;
        private Func<ParseResult<Nothing>, string> toString;
        private Func<ParseResult<string>, string> toString2;

        public ParseOptionalWsTest()
        {
            ws = Parser.ParseOptionalWhiteSpace;
            a = Parser.ParseExact("a", StringComparison.InvariantCultureIgnoreCase).WithOptionalLeadingWhiteSpace();

            toString = Utility.GetParseResultStringConverter<Nothing>(n => "null");
            toString2 = Utility.GetParseResultStringConverter<string>(s => s.Quoted());
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

        [TestMethod]
        public void TestA()
        {
            Assert.AreEqual("{ success, pos = 0, len = 1, value = \"A\" }", toString2(CharParserContext.TryParse(a, "A")));
        }

        [TestMethod]
        public void TestSpaceA()
        {
            Assert.AreEqual("{ success, pos = 0, len = 2, value = \"A\" }", toString2(CharParserContext.TryParse(a, " A")));
        }

        [TestMethod]
        public void TestSpaceB()
        {
            Assert.AreEqual("{ failure, { pos = 1, message = \"Expected \\\"a\\\"\" } }", toString2(CharParserContext.TryParse(a, " B")));
        }
    }
}
