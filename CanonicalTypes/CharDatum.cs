using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes
{
    public class CharDatum : Datum
    {
        private char ch;

        public CharDatum(char ch)
        {
            this.ch = ch;
        }

        public char Value => ch;

        public override DatumType DatumType => DatumType.Char;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitChar(this);

        public override T Visit<T>(IDatumVisitorWithState<T> visitor, T state) => visitor.VisitChar(state, this);
    }
}
