using System;

namespace CanonicalTypes
{
    public class BooleanDatum : Datum
    {
        private bool value;

        public BooleanDatum(bool value)
        {
            this.value = value;
        }

        public bool Value { get { return value; } }

        public override DatumType DatumType { get { return DatumType.Boolean; } }

        public override T Visit<T>(IDatumVisitor<T> visitor)
        {
            return visitor.VisitBoolean(this);
        }
    }
}
