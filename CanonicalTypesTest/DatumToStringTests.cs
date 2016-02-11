using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CanonicalTypes;

namespace CanonicalTypesTest
{
    [TestClass]
    public class DatumToStringTests
    {
        [TestMethod]
        public void TestBooleanToString()
        {
            Datum d = BooleanDatum.True;

            Assert.AreEqual("#t", d.ToString());
        }

        [TestMethod]
        public void TestNamedCharToString()
        {
            Datum d = new CharDatum('\r');

            Assert.AreEqual("#\\return", d.ToString());
        }

        [TestMethod]
        public void TestStringToString()
        {
            Datum d = new StringDatum("Hello\r\n");

            Assert.AreEqual("\"Hello\\r\\n\"", d.ToString());
        }
    }
}
