using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sunlighter.CanonicalTypes
{
    public class DatumHashStringVisitor : IDatumVisitor<string>
    {
        private static DatumHashStringVisitor instance = new DatumHashStringVisitor();

        public static DatumHashStringVisitor Instance { get { return instance; } }

        private DatumHashStringVisitor() { }

        public string VisitNull(NullDatum d)
        {
            return "n";
        }

        public string VisitBoolean(BooleanDatum d)
        {
            return "b" + d.Value;
        }

        public string VisitChar(CharDatum d)
        {
            return "c" + d.Value;
        }

        public string VisitString(StringDatum d)
        {
            return "s" + d.Value;
        }

        public string VisitInt(IntDatum d)
        {
            return "i" + d.Value;
        }

        public string VisitFloat(FloatDatum d)
        {
            return "f" + Convert.ToBase64String(BitConverter.GetBytes(d.Value));
        }

        public string VisitByteArray(ByteArrayDatum d)
        {
            return "a" + Convert.ToBase64String(d.Bytes.ToArray());
        }

        public string VisitSymbol(SymbolDatum d)
        {
            if (d.IsInterned)
            {
                return "y" + d.Name;
            }
            else
            {
                return "u" + d.ID;
            }
        }

        public string VisitList(ListDatum d)
        {
            return "l(" + string.Join(",", d.Values.Select(i => i.Visit(this))) + ")";
        }

        public string VisitSet(SetDatum d)
        {
            return "w(" + string.Join(",", d.Values.Select(i => i.Visit(this))) + ")";
        }

        public string VisitDictionary(DictionaryDatum d)
        {
            return "d(" + string.Join(",", d.Values.Select(kvp => kvp.Key.Visit(this) + ":" + kvp.Value.Visit(this))) + ")";
        }

        public string VisitMutableBox(MutableBoxDatum d)
        {
            return "m" + d.ID;
        }

        public string VisitRational(RationalDatum d)
        {
            return $"r{d.Value.Numerator}/{d.Value.Denominator}";
        }

        public string VisitGuid(GuidDatum d)
        {
            return "g" + d.Value.ToString("N");
        }
    }
}
