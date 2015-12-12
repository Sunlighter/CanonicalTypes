using System;
using System.Numerics;

namespace CanonicalTypes
{
    public class IntDatum : Datum
    {
        private BigInteger value;

        public IntDatum(BigInteger value)
        {
            this.value = value;
        }

        public BigInteger Value { get { return value; } }

        public override DatumType DatumType { get { return DatumType.Int; } }

        public override T Visit<T>(IDatumVisitor<T> visitor)
        {
            return visitor.VisitInt(this);
        }
    }
}
