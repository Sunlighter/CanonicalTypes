using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CanonicalTypes;
using System.Collections.Immutable;
using System.Numerics;

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

        [TestMethod]
        public void ToStringFromByteArray()
        {
            Datum d = ByteArrayDatum.FromByteArray(new byte[] { 0xAB, 0xCD, 0xEF });

            Assert.AreEqual("#y(ABCDEF)", d.ToString());
        }

        [TestMethod]
        public void ToStringFromListOfIntAndRational()
        {
            Datum d = new ListDatum
            (
                new Datum[]
                {
                    new IntDatum(new BigInteger(13)),
                    new RationalDatum(new BigRational(new BigInteger(-1), new BigInteger(3))),
                    new ListDatum
                    (
                        new Datum[]
                        {
                            new IntDatum(new BigInteger(20)),
                            new IntDatum(new BigInteger(134))
                        }
                        .ToImmutableList()
                    )
                }.ToImmutableList()
            );

            Assert.AreEqual("(13 -1/3 (20 134))", d.ToString());
        }

        [TestMethod]
        public void ToStringFromMutableBoxesAndLists()
        {
            MutableBoxDatum d1 = new MutableBoxDatum(new StringDatum("hello"));
            MutableBoxDatum d2 = new MutableBoxDatum(NullDatum.Value);

            d2.Content = new ListDatum
            (
                new Datum[]
                {
                    d1,
                    BooleanDatum.False,
                    d2
                }
                .ToImmutableList()
            );

            Datum d = d2;

            Assert.AreEqual("#b[1]=(#b=\"hello\" #f #b[1])", d.ToString());
        }
    }
}
