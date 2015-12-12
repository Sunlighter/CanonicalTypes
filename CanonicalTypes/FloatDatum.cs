using System;

namespace CanonicalTypes
{
    public class FloatDatum : Datum
    {
        private double value;

        public FloatDatum(double value)
        {
            this.value = value;
        }

        public double Value { get { return value; } }

        public override DatumType DatumType { get { return DatumType.Float; } }

        public override T Visit<T>(IDatumVisitor<T> visitor)
        {
            return visitor.VisitFloat(this);
        }
    }
}
