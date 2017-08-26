using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sunlighter.CanonicalTypes.Parsing;

namespace CanonicalTypesTest
{
    [TestClass]
    public class QuotedStringTests
    {
        public QuotedStringTests()
        {
            // nothing
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void QuotedStringNull()
        {
            Assert.AreEqual("null", ((string)null).Quoted());
        }

        [TestMethod]
        public void QuotedStringEmpty()
        {
            Assert.AreEqual("\"\"", string.Empty.Quoted());
        }

        [TestMethod]
        public void QuotedStringWithNewline()
        {
            Assert.AreEqual("\"test\\r\\n\"", "test\r\n".Quoted());
        }


        [TestMethod]
        public void QuotedStringWithNullCharacters()
        {
            Assert.AreEqual("\"\\x00\\x00\\x00\"", new string('\x00', 3).Quoted());
        }

        [TestMethod]
        public void QuotedStringWithUnicodeEscape()
        {
            Assert.AreEqual("\"\\uFEFF\"", "\uFEFF".Quoted());
        }

        [TestMethod]
        public void QuotedStringWithQuoteAndBackslash()
        {
            Assert.AreEqual("\"\\\"\\\\\"", "\"\\".Quoted());
        }
    }
}
