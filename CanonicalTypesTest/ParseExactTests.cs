using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CanonicalTypes.Parsing;

namespace CanonicalTypesTest
{
    [TestClass]
    public class ParseExactTests
    {
        private readonly ICharParser<string> parser;
        private readonly Func<ParseResult<string>, string> toString;

        public ParseExactTests()
        {
            parser = Parser.ParseExact("abc", StringComparison.InvariantCultureIgnoreCase);
            toString = Utility.GetParseResultStringConverter<string>(f => f.Quoted());
        }

        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        [TestMethod]
        public void ParseExact()
        {
            Assert.AreEqual("{ success, pos = 0, len = 3, value = \"Abc\" }", toString(CharParserContext.TryParse(parser, "Abc")));
        }

        [TestMethod]
        public void ParseExactWithExtraInput()
        {
            Assert.AreEqual("{ success, pos = 0, len = 3, value = \"ABC\" }", toString(CharParserContext.TryParse(parser, "ABCD")));
        }

        [TestMethod]
        public void ParseExactWithShortInput()
        {
            Assert.AreEqual("{ failure, { pos = 0, message = \"Expected \\\"abc\\\", found EOF\" } }", toString(CharParserContext.TryParse(parser, "AB")));
        }
    }
}
