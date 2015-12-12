using System;

namespace CanonicalTypes
{
    public interface IDatumVisitor<T>
    {
        T VisitNull(NullDatum d);
        T VisitBoolean(BooleanDatum d);
        T VisitChar(CharDatum d);
        T VisitString(StringDatum d);
        T VisitInt(IntDatum d);
        T VisitFloat(FloatDatum d);
        T VisitByteArray(ByteArrayDatum d);
        T VisitSymbol(SymbolDatum d);
        T VisitList(ListDatum d);
        T VisitSet(SetDatum d);
        T VisitDictionary(DictionaryDatum d);
        T VisitMutableBox(MutableBoxDatum d);
    }
}