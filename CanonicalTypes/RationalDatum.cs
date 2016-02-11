using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes
{
    public class RationalDatum : Datum
    {
        private BigRational value;

        public RationalDatum(BigRational value)
        {
            this.value = value;
        }

        public BigRational Value => value;

        public override DatumType DatumType => DatumType.Rational;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitRational(this);

        public override T Visit<T>(IDatumVisitorWithState<T> visitor, T state) => visitor.VisitRational(state, this);
    }
}
