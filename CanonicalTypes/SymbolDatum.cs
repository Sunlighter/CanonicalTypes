using System;

namespace CanonicalTypes
{
    public class SymbolDatum : Datum
    {
        private static object syncRoot = new object();
        private static int nextId;

        private string name;
        private int id;

        public SymbolDatum()
        {
            this.name = null;
            lock (syncRoot)
            {
                this.id = nextId;
                ++nextId;
            }
        }

        public SymbolDatum(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            this.name = name;
        }

        public bool IsInterned { get { return name != null; } }

        public string Name { get { if (name == null) throw new InvalidOperationException(); else return name; } }

        public int ID { get { if (name != null) throw new InvalidOperationException(); else return id; } }

        public override DatumType DatumType => DatumType.Symbol;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitSymbol(this);

        public override T Visit<T>(IDatumVisitorWithState<T> visitor, T state) => visitor.VisitSymbol(state, this);
    }
}
