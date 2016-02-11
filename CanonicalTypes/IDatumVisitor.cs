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
        T VisitRational(RationalDatum d);
        T VisitGuid(GuidDatum d);
    }

    public interface IDatumVisitorWithState<T>
    {
        T VisitNull(T state, NullDatum d);
        T VisitBoolean(T state, BooleanDatum d);
        T VisitChar(T state, CharDatum d);
        T VisitString(T state, StringDatum d);
        T VisitInt(T state, IntDatum d);
        T VisitFloat(T state, FloatDatum d);
        T VisitByteArray(T state, ByteArrayDatum d);
        T VisitSymbol(T state, SymbolDatum d);
        T VisitList(T state, ListDatum d);
        T VisitSet(T state, SetDatum d);
        T VisitDictionary(T state, DictionaryDatum d);
        T VisitMutableBox(T state, MutableBoxDatum d);
        T VisitRational(T state, RationalDatum d);
        T VisitGuid(T state, GuidDatum d);
    }
}