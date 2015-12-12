using System;

namespace CanonicalTypes
{
    public class NullDatum : Datum
    {
        private NullDatum() { }

        private static readonly NullDatum value = new NullDatum();

        public static NullDatum Value => value;

        public override DatumType DatumType => DatumType.Null;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitNull(this);
    }
}