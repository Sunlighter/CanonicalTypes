using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes
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

        public long ID { get { return id; } }

        public Datum Content
        {
            get { return content; }
            set { content = value; }
        }

        public override DatumType DatumType
        {
            get { return DatumType.MutableBox; }
        }

        public override T Visit<T>(IDatumVisitor<T> visitor)
        {
            return visitor.VisitMutableBox(this);
        }
    }
}
