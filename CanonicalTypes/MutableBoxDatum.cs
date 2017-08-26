using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunlighter.CanonicalTypes
{
    public class MutableBoxDatum : Datum
    {
        private static object syncRoot;
        private static long nextId;

        static MutableBoxDatum()
        {
            syncRoot = new object();
            nextId = 0L;
        }

        private long id;
        private Datum content;

        public MutableBoxDatum(Datum content)
        {
            lock(syncRoot)
            {
                this.id = nextId;
                ++nextId;
            }
        
            this.content = content;
        }

        public long ID => id;

        public Datum Content
        {
            get { return content; }
            set { content = value; }
        }

        public override DatumType DatumType => DatumType.MutableBox;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitMutableBox(this);

        public override T Visit<T>(IDatumVisitorWithState<T> visitor, T state) => visitor.VisitMutableBox(state, this);
    }
}
