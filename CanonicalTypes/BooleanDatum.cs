using System;

namespace CanonicalTypes
{
    public class BooleanDatum : Datum
    {
        private bool value;

        private BooleanDatum(bool value)
        {
            this.value = value;
        }

        private static readonly BooleanDatum vFalse = new BooleanDatum(false);
        private static readonly BooleanDatum vTrue = new BooleanDatum(true);

        public static BooleanDatum False => vFalse;
        public static BooleanDatum True => vTrue;

        public static BooleanDatum FromBoolean(bool value) => (value ? True : False);

        public bool Value { get { return value; } }

        public override DatumType DatumType { get { return DatumType.Boolean; } }

        public override T Visit<T>(IDatumVisitor<T> visitor)
        {
            return visitor.VisitBoolean(this);
        }
    }
}
