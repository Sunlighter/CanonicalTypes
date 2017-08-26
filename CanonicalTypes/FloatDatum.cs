using System;

namespace Sunlighter.CanonicalTypes
{
    public class FloatDatum : Datum
    {
        private double value;

        public FloatDatum(double value)
        {
            this.value = value;
        }

        public double Value { get { return value; } }

        public override DatumType DatumType => DatumType.Float;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitFloat(this);

        public override T Visit<T>(IDatumVisitorWithState<T> visitor, T state) => visitor.VisitFloat(state, this);
    }
}
