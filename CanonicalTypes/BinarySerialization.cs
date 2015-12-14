using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CanonicalTypes
{
    public static class BinarySerialization
    {
        private const byte B_NULL = 0;
        private const byte B_BOOLEAN_FALSE = 1;
        private const byte B_BOOLEAN_TRUE = 2;
        private const byte B_CHAR = 3;
        private const byte B_STRING = 4;
        private const byte B_INT = 5;
        private const byte B_FLOAT = 6;
        private const byte B_BYTE_ARRAY = 7;
        private const byte B_SYMBOL = 8;
        private const byte B_LIST = 9;
        private const byte B_SET = 10;
        private const byte B_DICTIONARY = 11;
        private const byte B_MUTABLE_BOX = 12;
        private const byte B_RATIONAL = 13;
        private const byte B_GUID = 14;

        private class BinaryWriteVisitor : IDatumVisitor<bool>
        {
            private BinaryWriter bw;
            private DictionaryDatum symbolMap;
            private DictionaryDatum mutableBoxMap;

            public BinaryWriteVisitor(BinaryWriter bw, DictionaryDatum symbolMap, DictionaryDatum mutableBoxMap)
            {
                this.bw = bw;
                this.symbolMap = symbolMap;
                this.mutableBoxMap = mutableBoxMap;
            }

            public bool VisitNull(NullDatum d)
            {
                bw.Write(B_NULL);
                return true;
            }

            public bool VisitBoolean(BooleanDatum d)
            {
                if (d.Value)
                {
                    bw.Write(B_BOOLEAN_TRUE);
                }
                else
                {
                    bw.Write(B_BOOLEAN_FALSE);
                }
                return true;
            }

            public bool VisitChar(CharDatum d)
            {
                bw.Write(B_CHAR);
                bw.Write(d.Value);
                return true;
            }

            public bool VisitString(StringDatum d)
            {
                bw.Write(B_STRING);
                bw.Write(d.Value);
                return true;
            }

            public bool VisitInt(IntDatum d)
            {
                bw.Write(B_INT);
                bw.WriteBigInteger(d.Value);
                return true;
            }

            public bool VisitFloat(FloatDatum d)
            {
                bw.Write(B_FLOAT);
                bw.Write(d.Value);
                return true;
            }

            public bool VisitByteArray(ByteArrayDatum d)
            {
                bw.Write(B_BYTE_ARRAY);
                bw.Write(d.Bytes.Length);
                byte[] temp = new byte[d.Bytes.Length];
                d.Bytes.CopyTo(temp);
                bw.Write(temp);
                return true;
            }

            public bool VisitSymbol(SymbolDatum d)
            {
                bw.Write(B_SYMBOL);
                BigInteger index = ((IntDatum)(symbolMap[d])).Value;
                if (index >= (BigInteger)(int.MinValue) && index <= (BigInteger)(int.MaxValue))
                {
                    bw.Write(unchecked((int)index));
                }
                else
                {
                    throw new IndexOutOfRangeException("Index doesn't fit in a 32-bit integer");
                }
                return true;
            }

            public bool VisitList(ListDatum d)
            {
                bw.Write(B_LIST);
                bw.Write(d.Count);
                foreach (Datum i in d.Values)
                {
                    i.Visit(this);
                }
                return true;
            }

            public bool VisitSet(SetDatum d)
            {
                bw.Write(B_SET);
                bw.Write(d.Count);
                foreach (Datum i in d.Values)
                {
                    i.Visit(this);
                }
                return true;
            }

            public bool VisitDictionary(DictionaryDatum d)
            {
                bw.Write(B_DICTIONARY);
                bw.Write(d.Count);
                foreach(KeyValuePair<Datum, Datum> kvp in d.Values)
                {
                    kvp.Key.Visit(this);
                    kvp.Value.Visit(this);
                }
                return true;
            }

            public bool VisitMutableBox(MutableBoxDatum d)
            {
                bw.Write(B_MUTABLE_BOX);
                IntDatum id = (IntDatum)(mutableBoxMap[d]);
                bw.Write((int)id.Value);
                return true;
            }

            public bool VisitRational(RationalDatum d)
            {
                bw.Write(B_RATIONAL);
                bw.WriteBigInteger(d.Value.Numerator);
                bw.WriteBigInteger(d.Value.Denominator);
                return true;
            }

            public bool VisitGuid(GuidDatum d)
            {
                bw.Write(B_GUID);
                byte[] buf = d.Value.ToByteArray();
                bw.Write(buf, 0, 16);
                return true;
            }
        }

        public static void WriteDatum(this BinaryWriter w, Datum d)
        {
            SymbolCollector sc = new SymbolCollector();
            SetDatum symbols = d.Visit(sc);

            MutableBoxCollector mc = new MutableBoxCollector();
            SetDatum mutableBoxes = d.Visit(mc);

            DictionaryDatum symbolMap = DictionaryDatum.Empty;
            ListDatum uninternedSymbols = ListDatum.Empty;
            ListDatum internedSymbols = ListDatum.Empty;
            DictionaryDatum mutableBoxMap = DictionaryDatum.Empty;
            
            foreach(SymbolDatum s in symbols.Cast<SymbolDatum>())
            {
                if (s.IsInterned)
                {
                    internedSymbols = internedSymbols.Add(s);
                }
                else
                {
                    uninternedSymbols = uninternedSymbols.Add(s);
                }
            }

            w.Write(uninternedSymbols.Count);
            foreach(int i in Enumerable.Range(0, uninternedSymbols.Count))
            {
                symbolMap = symbolMap.Add(uninternedSymbols[i], new IntDatum((BigInteger)(~i)));
            }

            w.Write(internedSymbols.Count);
            foreach(int i in Enumerable.Range(0, internedSymbols.Count))
            {
                symbolMap = symbolMap.Add(internedSymbols[i], new IntDatum((BigInteger)i));
                w.Write(((SymbolDatum)(internedSymbols[i])).Name);
            }

            w.Write(mutableBoxes.Count);
            foreach(int i in Enumerable.Range(0, mutableBoxes.Count))
            {
                mutableBoxMap = mutableBoxMap.Add(mutableBoxes[i], new IntDatum((BigInteger)i));
            }
            
            BinaryWriteVisitor v = new BinaryWriteVisitor(w, symbolMap, mutableBoxMap);

            foreach(int i in Enumerable.Range(0, mutableBoxes.Count))
            {
                ((MutableBoxDatum)mutableBoxes[i]).Content.Visit(v);
            }

            d.Visit(v);
        }

        public static Datum ReadDatum(this BinaryReader r)
        {
            int uninternedSymbolCount = r.ReadInt32();
            ListDatum uninternedSymbols = ListDatum.Empty;
            for(int i = 0; i < uninternedSymbolCount; ++i)
            {
                uninternedSymbols = uninternedSymbols.Add(new SymbolDatum());
            }
            int internedSymbolCount = r.ReadInt32();
            ListDatum internedSymbols = ListDatum.Empty;
            for (int i = 0; i < internedSymbolCount; ++i)
            {
                string name = r.ReadString();
                internedSymbols = internedSymbols.Add(new SymbolDatum(name));
            }
            int mutableBoxCount = r.ReadInt32();
            ListDatum mutableBoxes = ListDatum.Empty;
            for (int i = 0; i < mutableBoxCount; ++i)
            {
                mutableBoxes = mutableBoxes.Add(new MutableBoxDatum(NullDatum.Value));
            }

            Func<Datum> read = null;
            read = delegate()
            {
                byte typeId = r.ReadByte();
                switch (typeId)
                {
                    case B_NULL:
                        return NullDatum.Value;
                    case B_BOOLEAN_FALSE:
                        return BooleanDatum.False;
                    case B_BOOLEAN_TRUE:
                        return BooleanDatum.True;
                    case B_CHAR:
                        {
                            char ch = r.ReadChar();
                            return new CharDatum(ch);
                        }
                    case B_STRING:
                        {
                            string str = r.ReadString();
                            return new StringDatum(str);
                        }
                    case B_INT:
                        {
                            BigInteger b = r.ReadBigInteger();
                            return new IntDatum(b);
                        }
                    case B_FLOAT:
                        {
                            double f = r.ReadDouble();
                            return new FloatDatum(f);
                        }
                    case B_BYTE_ARRAY:
                        {
                            int nBytes = r.ReadInt32();
                            byte[] buf = new byte[nBytes];
                            int bytesRead = r.Read(buf, 0, nBytes);
                            if (bytesRead != nBytes) throw new EndOfStreamException();
                            return ByteArrayDatum.FromByteArray(buf);
                        }
                    case B_SYMBOL:
                        {
                            int index = r.ReadInt32();
                            if (index < 0) return uninternedSymbols[~index];
                            else return internedSymbols[index];
                        }
                    case B_LIST:
                        {
                            int count = r.ReadInt32();
                            ListDatum l = ListDatum.Empty;
                            foreach (int i in Enumerable.Range(0, count))
                            {
                                l = l.Add(read());
                            }
                            return l;
                        }
                    case B_SET:
                        {
                            int count = r.ReadInt32();
                            SetDatum s = SetDatum.Empty;
                            foreach (int i in Enumerable.Range(0, count))
                            {
                                s = s.Add(read());
                            }
                            return s;
                        }
                    case B_DICTIONARY:
                        {
                            int count = r.ReadInt32();
                            DictionaryDatum d = DictionaryDatum.Empty;
                            foreach (int i in Enumerable.Range(0, count))
                            {
                                Datum k = read();
                                Datum v = read();
                                d = d.Add(k, v);
                            }
                            return d;
                        }
                    case B_MUTABLE_BOX:
                        {
                            int index = r.ReadInt32();
                            return mutableBoxes[index];
                        }
                    case B_RATIONAL:
                        {
                            BigInteger numerator = r.ReadBigInteger();
                            BigInteger denominator = r.ReadBigInteger();
                            return new RationalDatum(new BigRational(numerator, denominator));
                        }
                    case B_GUID:
                        {
                            byte[] buf = new byte[16];
                            int bytesRead = r.Read(buf, 0, 16);
                            if (bytesRead < 16) throw new EndOfStreamException();
                            return new GuidDatum(new Guid(buf));
                        }
                    default:
                        throw new FormatException("Unknown type id");
                }
            };

            for (int j = 0; j < mutableBoxCount; ++j)
            {
                ((MutableBoxDatum)mutableBoxes[j]).Content = read();
            }

            return read();
        }

        public static byte[] SerializeToBytes(this Datum d)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8, false))
                {
                    bw.WriteDatum(d);
                }

                return ms.ToArray();
            }
        }

        public static Datum DeserializeToDatum(this byte[] b)
        {
            using (MemoryStream ms = new MemoryStream(b))
            {
                using (BinaryReader br = new BinaryReader(ms, Encoding.UTF8, false))
                {
                    return br.ReadDatum();
                }
            }
        }

        public static void WriteBigInteger(this BinaryWriter bw, BigInteger b)
        {
            byte[] intBytes = b.ToByteArray();
            bw.Write(intBytes.Length);
            bw.Write(intBytes);
        }

        public static BigInteger ReadBigInteger(this BinaryReader br)
        {

            int nBytes = br.ReadInt32();
            byte[] buf = new byte[nBytes];
            int bytesRead = br.Read(buf, 0, nBytes);
            if (bytesRead != nBytes) throw new EndOfStreamException();
            return new BigInteger(buf);
        }
    }
}
