using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes
{
    class MutableBoxCollector : IDatumVisitor<SetDatum>
    {
        private SetDatum visiting;
        private DictionaryDatum visited;

        public MutableBoxCollector()
        {
            this.visiting = SetDatum.Empty;
            this.visited = DictionaryDatum.Empty;
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
            return SetDatum.Empty;
        }

        public SetDatum VisitList(ListDatum d)
        {
            SetDatum result = SetDatum.Empty;
            
            foreach(Datum item in d)
            {
                result = SetDatum.Union(item.Visit(this), result);
            }

            return result;
        }

        public SetDatum VisitSet(SetDatum d)
        {
            SetDatum result = SetDatum.Empty;

            foreach(Datum item in d)
            {
                result = SetDatum.Union(item.Visit(this), result);
            }

            return result;
        }

        public SetDatum VisitDictionary(DictionaryDatum d)
        {
            SetDatum result = SetDatum.Empty;

            foreach(KeyValuePair<Datum, Datum> kvp in d)
            {
                result = SetDatum.Union(kvp.Key.Visit(this), result);
                result = SetDatum.Union(kvp.Value.Visit(this), result);
            }

            return result;
        }

        public SetDatum VisitMutableBox(MutableBoxDatum d)
        {
            if (visited.ContainsKey(d))
            {
                return (SetDatum)(visited[d]);
            }
            else if (visiting.Contains(d))
            {
                return SetDatum.Empty;
            }
            else
            {
                visiting = SetDatum.Union(visiting, SetDatum.Singleton(d));
                SetDatum result = SetDatum.Union(d.Content.Visit(this), SetDatum.Singleton(d));
                visiting = SetDatum.Difference(visiting, SetDatum.Singleton(d));
                visited = visited.Add(d, result);
                return result;
            }
        }

        public SetDatum VisitRational(RationalDatum d)
        {
            return SetDatum.Empty;
        }
    }
}
