﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CanonicalTypes;

namespace CanonicalTypesTest
{
    [TestClass]
    public class ToStringTests
    {
        [TestMethod]
        public void ToStringFromBoolean()
        {
            Datum d = BooleanDatum.True;

            Assert.AreEqual("#t", d.ToString());
        }

        [TestMethod]
        public void ToStringFromNamedChar()
        {
            Datum d = new CharDatum('\r');

            Assert.AreEqual("#\\return", d.ToString());
        }

        [TestMethod]
        public void ToStringFromHexChar()
        {
            Datum d = new CharDatum('\xFEFF');

            Assert.AreEqual("#\\xFEFF", d.ToString());
        }

        [TestMethod]
        public void ToStringFromString()
        {
            Datum d = new StringDatum("Hello\r\n");

            Assert.AreEqual("\"Hello\\r\\n\"", d.ToString());
        }

        [TestMethod]
        public void ToStringFromDouble()
        {
            Datum d = new FloatDatum(11.1);

            Assert.AreEqual("11.1", d.ToString());
        }
    }
}
