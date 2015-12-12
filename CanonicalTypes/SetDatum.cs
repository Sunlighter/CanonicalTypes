using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CanonicalTypes
{
    public class SetDatum : Datum, IEnumerable<Datum>
    {
        private ImmutableSortedSet<Datum> set;

        private SetDatum(ImmutableSortedSet<Datum> set)
        {
            this.set = set;
        }

        public static SetDatum FromEnumerable(IEnumerable<Datum> items)
        {
            var idb = ImmutableSortedSet<Datum>.Empty.ToBuilder();
            idb.KeyComparer = DatumComparer.Instance;
            idb.UnionWith(items);
            return new SetDatum(idb.ToImmutable());
        }

        public ImmutableSortedSet<Datum> Values => set;

        public override DatumType DatumType => DatumType.Set;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitSet(this);

        private static SetDatum empty = FromEnumerable(Enumerable.Empty<Datum>());

        public static SetDatum Empty => empty;

        public bool Contains(Datum value) => set.Contains(value);

        public int Count => set.Count;

        public Datum this[int index] => set[index];

        public Datum Min => set.Min;

        public Datum Max => set.Max;

        private ImmutableSortedSet<Datum>.Builder ToBuilder()
        {
            var idb = ImmutableSortedSet<Datum>.Empty.ToBuilder();
            idb.KeyComparer = DatumComparer.Instance;
            idb.UnionWith(set);
            return idb;
        }

        public static SetDatum Singleton(Datum d)
        {
            var idb = ImmutableSortedSet<Datum>.Empty.ToBuilder();
            idb.KeyComparer = DatumComparer.Instance;
            idb.Add(d);
            return new SetDatum(idb.ToImmutable());
        }

        public SetDatum Add(Datum d)
        {
            return new SetDatum(set.Add(d));
        }

        public SetDatum Remove(Datum d)
        {
            return new SetDatum(set.Remove(d));
        }

        public static SetDatum Union(SetDatum a, SetDatum b)
        {
            var idb = a.ToBuilder();
            idb.UnionWith(b.set);
            return new SetDatum(idb.ToImmutable());
        }

        public static SetDatum UnionAll(IEnumerable<SetDatum> a)
        {
            ImmutableSortedSet<Datum>.Builder b = null;
            foreach(SetDatum aitem in a)
            {
                if (b == null)
                {
                    b = aitem.ToBuilder();
                }
                else
                {
                    b.UnionWith(aitem.set);
                }
            }
            if (b == null)
            {
                return SetDatum.Empty;
            }
            else
            {
                return new SetDatum(b.ToImmutable());
            }
        }

        public static SetDatum Intersection(SetDatum a, SetDatum b)
        {
            var idb = a.ToBuilder();
            idb.IntersectWith(b.set);
            return new SetDatum(idb.ToImmutable());
        }

        public static SetDatum Difference(SetDatum a, SetDatum b)
        {
            var idb = a.ToBuilder();
            idb.ExceptWith(b.set);
            return new SetDatum(idb.ToImmutable());
        }

        public static SetDatum SymmetricDifference(SetDatum a, SetDatum b)
        {
            var idb = a.ToBuilder();
            idb.SymmetricExceptWith(b.set);
            return new SetDatum(idb.ToImmutable());
        }

        public IEnumerator<Datum> GetEnumerator()
        {
            return set.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)set).GetEnumerator();
        }
    }
}
