using System;

namespace CanonicalTypes
{
    public class StringDatum : Datum
    {
        private string value;

        public StringDatum(string value)
        {
            this.value = value;
        }

        public string Value => value;

        public override DatumType DatumType => DatumType.String;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitString(this);

        public override T Visit<T>(IDatumVisitorWithState<T> visitor, T state) => visitor.VisitString(state, this);
    }
}
