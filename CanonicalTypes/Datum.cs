using System;

namespace CanonicalTypes
{
    public abstract class Datum
    {
        public abstract DatumType DatumType { get; }

        public abstract T Visit<T>(IDatumVisitor<T> visitor);
    }
}