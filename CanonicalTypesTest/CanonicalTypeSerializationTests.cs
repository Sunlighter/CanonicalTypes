using CanonicalTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace CanonicalTypesTest
{
    [TestClass]
    public class CanonicalTypeSerializationTests
    {
        [TestMethod]
        public void SerializeMutableBoxes()
        {
            MutableBoxDatum m1 = new MutableBoxDatum(NullDatum.Value);

            MutableBoxDatum m2 = new MutableBoxDatum(NullDatum.Value);

            Datum d1 = ListDatum.Empty
                .Add(new IntDatum(100))
                .Add(BooleanDatum.True)
                .Add(m1);

            Datum d2 = DictionaryDatum.Empty
                .Add(new SymbolDatum("k1"), d1)
                .Add(new SymbolDatum("k2"), m2);

            m1.Content = d2;
            m2.Content = d1;

            byte[] b0 = d1.SerializeToBytes();

            Datum d3 = b0.DeserializeToDatum();

            byte[] b1 = d3.SerializeToBytes();

        }
    }
}
