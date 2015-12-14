using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes.Parsing
{
    public static partial class Utility
    {
        public static bool IsValidRange(string str, int off, int len)
        {
            if (off < 0) return false;
            if (off > str.Length) return false;
            if (len < 0) return false;
            if ((long)off + (long)len > (long)(int.MaxValue)) return false;
            if (off + len > str.Length) return false;

            return true;
        }

    }
}
