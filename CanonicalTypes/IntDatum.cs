using System;
using System.Numerics;

namespace Sunlighter.CanonicalTypes
{
    public class IntDatum : Datum
    {
        private BigInteger value;

        public IntDatum(BigInteger value)
        {
            this.value = value;
        }

        public BigInteger Value => value;

        public override DatumType DatumType => DatumType.Int;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitInt(this);

        public override T Visit<T>(IDatumVisitorWithState<T> visitor, T state) => visitor.VisitInt(state, this);
    }
}
