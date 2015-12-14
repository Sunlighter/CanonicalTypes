using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes.Parsing
{
    public struct PosLenId : IEquatable<PosLenId>, IComparable<PosLenId>
    {
        private int pos;
        private int len;
        private long id;

        public PosLenId(int pos, int len, long id)
        {
            this.pos = pos;
            this.len = len;
            this.id = id;
        }

        public int Position => pos;
        public int Length => len;
        public long Id => id;

        public override string ToString()
        {
            return $"{{ pos = {pos}, len = {len}, id = {id} }}";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PosLenId)
            {
                PosLenId pObj = (PosLenId)obj;
                return (pos == pObj.pos) && (len == pObj.len) && (id == pObj.id);
            }
            else return false;
        }

        public bool Equals(PosLenId other)
        {
            return (pos == other.pos) && (len == other.len) && (id == other.id);
        }

        public int CompareTo(PosLenId other)
        {
            if (pos < other.pos) return -1;
            if (pos > other.pos) return 1;
            if (len < other.len) return -1;
            if (len > other.len) return 1;
            if (id < other.id) return -1;
            if (id > other.id) return 1;
            return 0;
        }

        public static bool operator == (PosLenId a, PosLenId b)
        {
            return (a.pos == b.pos) && (a.len == b.len) && (a.id == b.id);
        }

        public static bool operator < (PosLenId a, PosLenId b)
        {
            if (a.pos < b.pos) return true;
            if (a.pos > b.pos) return false;
            if (a.len < b.len) return true;
            if (a.len > b.len) return false;
            if (a.id < b.id) return true;
            return false;
        }

        public static bool operator !=(PosLenId a, PosLenId b) { return !(a == b); }

        public static bool operator >(PosLenId a, PosLenId b) { return (b < a); }

        public static bool operator >=(PosLenId a, PosLenId b) { return !(a < b); }

        public static bool operator <=(PosLenId a, PosLenId b) { return !(b < a); }
    }
}
