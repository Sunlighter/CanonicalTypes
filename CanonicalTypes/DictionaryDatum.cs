using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace CanonicalTypes
{
    public class DictionaryDatum : Datum, IEnumerable<KeyValuePair<Datum, Datum>>
    {
        private ImmutableSortedDictionary<Datum, Datum> dict;

        private DictionaryDatum(ImmutableSortedDictionary<Datum, Datum> dict)
        {
            this.dict = dict;
            this.keys = new Lazy<SetDatum>(GetKeys, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public static DictionaryDatum FromEnumerable(IEnumerable<Tuple<Datum, Datum>> tuples)
        {
            var idb = ImmutableSortedDictionary<Datum, Datum>.Empty.ToBuilder();
            idb.KeyComparer = DatumComparer.Instance;
            idb.ValueComparer = DatumEqualityComparer.Instance;
            idb.AddRange(tuples.Select(i => new KeyValuePair<Datum, Datum>(i.Item1, i.Item2)));
            return new DictionaryDatum(idb.ToImmutable());
        }

        public static DictionaryDatum FromEnumerable(IEnumerable<KeyValuePair<Datum, Datum>> kvps)
        {
            var idb = ImmutableSortedDictionary<Datum, Datum>.Empty.ToBuilder();
            idb.KeyComparer = DatumComparer.Instance;
            idb.ValueComparer = DatumEqualityComparer.Instance;
            idb.AddRange(kvps);
            return new DictionaryDatum(idb.ToImmutable());
        }

        private static DictionaryDatum empty = DictionaryDatum.FromEnumerable(Enumerable.Empty<KeyValuePair<Datum, Datum>>());

        public static DictionaryDatum Empty { get { return empty; } }

        public int Count { get { return dict.Count; } }

        public ImmutableSortedDictionary<Datum, Datum> Values { get { return dict; } }

        public override DatumType DatumType { get { return DatumType.Dictionary; } }

        public override T Visit<T>(IDatumVisitor<T> visitor)
        {
            return visitor.VisitDictionary(this);
        }

        public bool ContainsKey(Datum d)
        {
            return dict.ContainsKey(d);
        }

        /// <summary>
        /// Adds or replaces.
        /// </summary>
        /// <param name="k">Key</param>
        /// <param name="v">Value</param>
        /// <returns>New dictionary with the (key, value) pair added or replaced.</returns>
        public DictionaryDatum Add(Datum k, Datum v)
        {
            return new DictionaryDatum(dict.SetItem(k, v));
        }

        public DictionaryDatum Remove(Datum k)
        {
            return new DictionaryDatum(dict.Remove(k));
        }

        public Datum this[Datum k]
        {
            get
            {
                return dict[k];
            }
        }

        public Option<Datum> TryGet(Datum k)
        {
            if (dict.ContainsKey(k))
            {
                return Option<Datum>.Some(dict[k]);
            }
            else
            {
                return Option<Datum>.None;
            }
        }

        public static IEnumerable<JoinResult<Datum, Datum, Datum>> InnerJoin(DictionaryDatum left, DictionaryDatum right)
        {
            SetDatum keys = SetDatum.Intersection(left.Keys, right.Keys);
            return keys.Select(k => new JoinResult<Datum, Datum, Datum>(k, left[k], right[k]));
        }

        public static IEnumerable<JoinResult<Datum, Datum, Option<Datum>>> LeftJoin(DictionaryDatum left, DictionaryDatum right)
        {
            return left.Keys.Select(k => new JoinResult<Datum, Datum, Option<Datum>>(k, left[k], right.TryGet(k)));
        }

        public static IEnumerable<JoinResult<Datum, Option<Datum>, Datum>> RightJoin(DictionaryDatum left, DictionaryDatum right)
        {
            return right.Keys.Select(k => new JoinResult<Datum, Option<Datum>, Datum>(k, left.TryGet(k), right[k]));
        }

        public static IEnumerable<JoinResult<Datum, Option<Datum>, Option<Datum>>> FullJoin(DictionaryDatum left, DictionaryDatum right)
        {
            SetDatum keys = SetDatum.Union(left.Keys, right.Keys);
            return keys.Select(k => new JoinResult<Datum, Option<Datum>, Option<Datum>>(k, left.TryGet(k), right.TryGet(k)));
        }

        private SetDatum GetKeys()
        {
            return SetDatum.FromEnumerable(dict.Keys);
        }

        private Lazy<SetDatum> keys;

        public SetDatum Keys
        {
            get
            {
                return keys.Value;
            }
        }

        public KeyValuePair<Datum, Datum> this[int index]
        {
            get
            {
                var key = this.Keys[index];
                return new KeyValuePair<Datum, Datum>(key, dict[key]);
            }
        }

        public IEnumerator<KeyValuePair<Datum, Datum>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)dict).GetEnumerator();
        }
    }
}
