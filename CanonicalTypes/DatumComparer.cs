using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace Sunlighter.CanonicalTypes
{
    public class DatumComparer : IComparer<Datum>
    {
        private static DatumComparer instance = new DatumComparer();

        public static DatumComparer Instance { get { return instance; } }

        private DatumComparer() { }

        private int CompareBoolean(BooleanDatum x, BooleanDatum y)
        {
            return x.Value ? (y.Value ? 0 : 1) : (y.Value ? -1 : 0);
        }

        private int CompareChar(CharDatum x, CharDatum y)
        {
            return (x.Value < y.Value) ? -1 : (x.Value > y.Value) ? 1 : 0;
        }

        private int CompareString(StringDatum x, StringDatum y)
        {
            return string.Compare(x.Value, y.Value, StringComparison.Ordinal);
        }

        private int CompareInt(IntDatum x, IntDatum y)
        {
            return BigInteger.Compare(x.Value, y.Value);
        }

        private int CompareFloat(FloatDatum x, FloatDatum y)
        {
            return Comparer<long>.Default.Compare
            (
                BitConverter.DoubleToInt64Bits(x.Value),
                BitConverter.DoubleToInt64Bits(y.Value)
            );
        }

        private int CompareSymbol(SymbolDatum x, SymbolDatum y)
        {
            if (!x.IsInterned)
            {
                if (!y.IsInterned)
                {
                    return Comparer<int>.Default.Compare(x.ID, y.ID);
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (!y.IsInterned)
                {
                    return 1;
                }
                else
                {
                    return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                }
            }
        }

        private int CompareList(ListDatum x, ListDatum y)
        {
            int pos = 0;
            while(true)
            {
                if (x.Count == pos && y.Count == pos) return 0;
                if (x.Count == pos) return -1;
                if (y.Count == pos) return 1;
                int itemResult = this.Compare(x[pos], y[pos]);
                if (itemResult != 0) return itemResult;
                ++pos;
            }
        }

        private int CompareSet(SetDatum x, SetDatum y)
        {
            int pos = 0;
            while (true)
            {
                if (x.Count == pos && y.Count == pos) return 0;
                if (x.Count == pos) return -1;
                if (y.Count == pos) return 1;
                int itemResult = this.Compare(x[pos], y[pos]);
                if (itemResult != 0) return itemResult;
                ++pos;
            }
        }

        private int CompareDictionary(DictionaryDatum x, DictionaryDatum y)
        {
            int pos = 0;
            while (true)
            {
                if (x.Count == pos && y.Count == pos) return 0;
                if (x.Count == pos) return -1;
                if (y.Count == pos) return 1;
                KeyValuePair<Datum, Datum> xkvp = x[pos];
                KeyValuePair<Datum, Datum> ykvp = y[pos];
                int itemResult = this.Compare(xkvp.Key, ykvp.Key);
                if (itemResult != 0) return itemResult;
                itemResult = this.Compare(xkvp.Value, ykvp.Value);
                if (itemResult != 0) return itemResult;
                ++pos;
            }
        }

        private int CompareByteArray(ByteArrayDatum x, ByteArrayDatum y)
        {
            int pos = 0;
            while(true)
            {
                if (x.Length == pos && y.Length == pos) return 0;
                if (x.Length == pos) return -1;
                if (y.Length == pos) return 1;
                int itemResult = Comparer<byte>.Default.Compare(x[pos], y[pos]);
                if (itemResult != 0) return itemResult;
                ++pos;
            }
        }

        private int CompareMutableBox(MutableBoxDatum x, MutableBoxDatum y)
        {
            if (x.ID < y.ID) return -1;
            if (x.ID > y.ID) return 1;
            return 0;
        }

        private int CompareRational(RationalDatum x, RationalDatum y)
        {
            if (x.Value < y.Value) return -1;
            if (x.Value > y.Value) return 1;
            return 0;
        }

        private int CompareGuid(GuidDatum x, GuidDatum y)
        {
            byte[] xb = x.Value.ToByteArray();
            byte[] yb = y.Value.ToByteArray();
            for(int i = 0; i < 16; ++i)
            {
                if (xb[i] < yb[i]) return -1;
                if (xb[i] > yb[i]) return 1;
            }
            return 0;
        }

        public int Compare(Datum x, Datum y)
        {
            if (x.DatumType < y.DatumType) return -1;
            if (x.DatumType > y.DatumType) return 1;

            switch(x.DatumType)
            {
                case DatumType.Null:
                    return 0;
                case DatumType.Boolean:
                    return CompareBoolean((BooleanDatum)x, (BooleanDatum)y);
                case DatumType.Char:
                    return CompareChar((CharDatum)x, (CharDatum)y);
                case DatumType.String:
                    return CompareString((StringDatum)x, (StringDatum)y);
                case DatumType.Int:
                    return CompareInt((IntDatum)x, (IntDatum)y);
                case DatumType.Float:
                    return CompareFloat((FloatDatum)x, (FloatDatum)y);
                case DatumType.ByteArray:
                    return CompareByteArray((ByteArrayDatum)x, (ByteArrayDatum)y);
                case DatumType.Symbol:
                    return CompareSymbol((SymbolDatum)x, (SymbolDatum)y);
                case DatumType.List:
                    return CompareList((ListDatum)x, (ListDatum)y);
                case DatumType.Set:
                    return CompareSet((SetDatum)x, (SetDatum)y);
                case DatumType.Dictionary:
                    return CompareDictionary((DictionaryDatum)x, (DictionaryDatum)y);
                case DatumType.MutableBox:
                    return CompareMutableBox((MutableBoxDatum)x, (MutableBoxDatum)y);
                case DatumType.Rational:
                    return CompareRational((RationalDatum)x, (RationalDatum)y);
                case DatumType.Guid:
                    return CompareGuid((GuidDatum)x, (GuidDatum)y);
                default:
                    throw new ArgumentException("Unexpected DatumType");
            }
        }
    }
}
