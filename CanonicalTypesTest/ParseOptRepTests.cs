using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CanonicalTypes.Parsing;
using System.Collections.Immutable;
using System.Linq;

namespace CanonicalTypesTest
{
    [TestClass]
    public class ParseOptRepTests
    {
        private ICharParser<ImmutableList<string>> parserOpt;
        private ICharParser<ImmutableList<string>> parserRep;
        private ICharParser<ImmutableList<string>> parserOptRep;
        private readonly Func<ParseResult<ImmutableList<string>>, string> toString;

        public ParseOptRepTests()
        {
            parserOpt = Parser.ParseOptRep(Parser.ParseExact("abc", StringComparison.InvariantCultureIgnoreCase), true, false);
            parserRep = Parser.ParseOptRep(Parser.ParseExact("abc", StringComparison.InvariantCultureIgnoreCase), false, true);
            parserOptRep = Parser.ParseOptRep(Parser.ParseExact("abc", StringComparison.InvariantCultureIgnoreCase), true, true);
            var listToString = Utility.GetImmutableListStringConverter<string>(i => i.Quoted());
            toString = Utility.GetParseResultStringConverter(listToString);
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
        public void ParseOptWithNone()
        {
            Assert.AreEqual("{ success, pos = 0, len = 0, value = [ ] }", toString(CharParserContext.TryParse(parserOpt, "de")));
        }

        [TestMethod]
        public void ParseOptWithOne()
        {
            Assert.AreEqual("{ success, pos = 0, len = 3, value = [ \"abc\" ] }", toString(CharParserContext.TryParse(parserOpt, "abcde")));
        }

        [TestMethod]
        public void ParseOptWithTwo()
        {
            Assert.AreEqual("{ success, pos = 0, len = 3, value = [ \"abc\" ] }", toString(CharParserContext.TryParse(parserOpt, "abcabcde")));
        }

        [TestMethod]
        public void ParseRepWithNone()
        {
            Assert.AreEqual("{ failure, { pos = 0, message = \"Expected \\\"abc\\\", found EOF\" } }", toString(CharParserContext.TryParse(parserRep, "de")));
        }

        [TestMethod]
        public void ParseRepWithOne()
        {
            Assert.AreEqual("{ success, pos = 0, len = 3, value = [ \"abc\" ] }", toString(CharParserContext.TryParse(parserRep, "abcde")));
        }

        [TestMethod]
        public void ParseRepWithTwo()
        {
            Assert.AreEqual("{ success, pos = 0, len = 6, value = [ \"abc\", \"abc\" ] }", toString(CharParserContext.TryParse(parserRep, "abcabcde")));
        }

        [TestMethod]
        public void ParseOptRepWithNone()
        {
            Assert.AreEqual("{ success, pos = 0, len = 0, value = [ ] }", toString(CharParserContext.TryParse(parserOptRep, "de")));
        }

        [TestMethod]
        public void ParseOptRepWithOne()
        {
            Assert.AreEqual("{ success, pos = 0, len = 3, value = [ \"abc\" ] }", toString(CharParserContext.TryParse(parserOptRep, "abcde")));
        }

        [TestMethod]
        public void ParseOptRepWithTwo()
        {
            Assert.AreEqual("{ success, pos = 0, len = 6, value = [ \"abc\", \"abc\" ] }", toString(CharParserContext.TryParse(parserOptRep, "abcabcde")));
        }
    }
}
