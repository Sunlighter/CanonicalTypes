using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes
{
    public class SymbolCollector : IDatumVisitor<SetDatum>
    {
        private ObjectIDGenerator idgen;
        private ImmutableDictionary<long, SetDatum> mutableBoxCollections;

        public SymbolCollector()
        {
            idgen = new ObjectIDGenerator();
            mutableBoxCollections = ImmutableDictionary<long, SetDatum>.Empty;
        }

        public SetDatum VisitNull(NullDatum d)
        {
            return SetDatum.Empty;
        }

        public SetDatum VisitBoolean(BooleanDatum d)
        {
            return SetDatum.Empty;
        }

        public SetDatum VisitChar(CharDatum d)
        {
            return SetDatum.Empty;
        }

        public SetDatum VisitString(StringDatum d)
        {
            return SetDatum.Empty;
        }

        public SetDatum VisitInt(IntDatum d)
        {
            return SetDatum.Empty;
        }

        public SetDatum VisitFloat(FloatDatum d)
        {
            return SetDatum.Empty;
        }

        public SetDatum VisitByteArray(ByteArrayDatum d)
        {
            return SetDatum.Empty;
        }

        public SetDatum VisitSymbol(SymbolDatum d)
        {
            return SetDatum.Singleton(d);
        }

        public SetDatum VisitList(ListDatum d)
        {
            return SetDatum.UnionAll(d.Values.Select(i => i.Visit(this)));
        }

        public SetDatum VisitSet(SetDatum d)
        {
            return SetDatum.UnionAll(d.Values.Select(i => i.Visit(this)));
        }

        public SetDatum VisitDictionary(DictionaryDatum d)
        {
            return SetDatum.UnionAll
            (
                d.Values.Select(i => SetDatum.Union(i.Key.Visit(this), i.Value.Visit(this)))
            );
        }

        public SetDatum VisitMutableBox(MutableBoxDatum d)
        {
            bool firstTime;
            long id = idgen.GetId(d, out firstTime);
            if (firstTime)
            {
                SetDatum s = d.Content.Visit(this);
                mutableBoxCollections = mutableBoxCollections.Add(id, s);
                return s;
            }
            else if (mutableBoxCollections.ContainsKey(id))
            {
                return mutableBoxCollections[id];
            }
            else
            {
                return SetDatum.Empty;
            }
        }

        public SetDatum VisitRational(RationalDatum d)
        {
            return SetDatum.Empty;
        }
    }
}
