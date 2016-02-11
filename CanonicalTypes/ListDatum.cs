using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CanonicalTypes
{
    public class ListDatum : Datum, IEnumerable<Datum>
    {
        private ImmutableList<Datum> values;

        public ListDatum(ImmutableList<Datum> values)
        {
            this.values = values;
        }

        public ImmutableList<Datum> Values => values;

        public override DatumType DatumType => DatumType.List;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitList(this);

        public override T Visit<T>(IDatumVisitorWithState<T> visitor, T state) => visitor.VisitList(state, this);

        private static ListDatum empty = new ListDatum(ImmutableList<Datum>.Empty);

        public static ListDatum Empty => empty;

        public Datum this[int index] => values[index];

        public int Count => values.Count;

        public ListDatum Add(Datum d)
        {
            return new ListDatum(values.Add(d));
        }

        public ListDatum Insert(int i, Datum d)
        {
            return new ListDatum(values.Insert(i, d));
        }

        public IEnumerator<Datum> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)values).GetEnumerator();
        }
    }
}
