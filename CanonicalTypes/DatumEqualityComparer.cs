using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CanonicalTypes
{
    public class DatumEqualityComparer : IEqualityComparer<Datum>
    {
        private static DatumEqualityComparer instance = new DatumEqualityComparer();

        public static DatumEqualityComparer Instance { get { return instance; } }

        private DatumEqualityComparer() { }

        private bool EqualBoolean(BooleanDatum x, BooleanDatum y)
        {
            return x.Value == y.Value;
        }

        private bool EqualChar(CharDatum x, CharDatum y)
        {
            return x.Value == y.Value;
        }

        private bool EqualString(StringDatum x, StringDatum y)
        {
            return string.Compare
            (
                x.Value,
                y.Value,
                StringComparison.Ordinal
            ) == 0;
        }

        private bool EqualInt(IntDatum x, IntDatum y)
        {
            return x.Value == y.Value;
        }

        private bool EqualFloat(FloatDatum x, FloatDatum y)
        {
            return string.Compare
            (
                Convert.ToBase64String(BitConverter.GetBytes(x.Value)),
                Convert.ToBase64String(BitConverter.GetBytes(y.Value)),
                StringComparison.Ordinal
            ) == 0;
        }

        private bool EqualByteArray(ByteArrayDatum x, ByteArrayDatum y)
        {
            if (x.Length != y.Length) return false;
            foreach(int i in Enumerable.Range(0, x.Length))
            {
                if (x[i] != y[i]) return false;
            }
            return true;
        }

        private bool EqualSymbol(SymbolDatum x, SymbolDatum y)
        {
            if (x.IsInterned != y.IsInterned) return false;
            if (x.IsInterned)
            {
                return string.Compare
                (
                    x.Name,
                    y.Name,
                    StringComparison.Ordinal
                ) == 0;
            }
            else
            {
                return x.ID == y.ID;
            }
        }

        private bool EqualList(ListDatum x, ListDatum y)
        {
            if (x.Count != y.Count) return false;
            foreach (int i in Enumerable.Range(0, x.Count))
            {
                if (!Equals(x[i], y[i])) return false;
            }
            return true;
        }

        private bool EqualSet(SetDatum x, SetDatum y)
        {
            if (x.Count != y.Count) return false;
            foreach(int i in Enumerable.Range(0, x.Count))
            {
                if (!Equals(x[i], y[i])) return false;
            }
            return true;
        }

        private bool EqualDictionary(DictionaryDatum x, DictionaryDatum y)
        {
            if (x.Count != y.Count) return false;
            foreach(int i in Enumerable.Range(0, x.Count))
            {
                var kx = x[i];
                var ky = y[i];
                if (!Equals(kx.Key, ky.Key)) return false;
                if (!Equals(kx.Value, ky.Value)) return false;
            }
            return true;
        }

        private bool EqualMutableBox(MutableBoxDatum x, MutableBoxDatum y)
        {
            return x.ID == y.ID;
        }

        private bool EqualRational(RationalDatum x, RationalDatum y)
        {
            return x.Value == y.Value;
        }

        private bool EqualGuid(GuidDatum x, GuidDatum y)
        {
            return x.Value == y.Value;
        }

        public bool Equals(Datum x, Datum y)
        {
            if (x.DatumType != y.DatumType) return false;

            switch (x.DatumType)
            {
                case DatumType.Null:
                    return true;
                case DatumType.Boolean:
                    return EqualBoolean((BooleanDatum)x, (BooleanDatum)y);
                case DatumType.Char:
                    return EqualChar((CharDatum)x, (CharDatum)y);
                case DatumType.String:
                    return EqualString((StringDatum)x, (StringDatum)y);
                case DatumType.Int:
                    return EqualInt((IntDatum)x, (IntDatum)y);
                case DatumType.Float:
                    return EqualFloat((FloatDatum)x, (FloatDatum)y);
                case DatumType.ByteArray:
                    return EqualByteArray((ByteArrayDatum)x, (ByteArrayDatum)y);
                case DatumType.Symbol:
                    return EqualSymbol((SymbolDatum)x, (SymbolDatum)y);
                case DatumType.List:
                    return EqualList((ListDatum)x, (ListDatum)y);
                case DatumType.Set:
                    return EqualSet((SetDatum)x, (SetDatum)y);
                case DatumType.Dictionary:
                    return EqualDictionary((DictionaryDatum)x, (DictionaryDatum)y);
                case DatumType.MutableBox:
                    return EqualMutableBox((MutableBoxDatum)x, (MutableBoxDatum)y);
                case DatumType.Rational:
                    return EqualRational((RationalDatum)x, (RationalDatum)y);
                case DatumType.Guid:
                    return EqualGuid((GuidDatum)x, (GuidDatum)y);
                default:
                    throw new ArgumentException();
            }
        }

        public int GetHashCode(Datum obj)
        {
            return obj.Visit(DatumHashStringVisitor.Instance).GetHashCode();
        }
    }
}
