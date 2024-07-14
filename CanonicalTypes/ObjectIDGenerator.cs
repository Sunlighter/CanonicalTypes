using System;
using System.Collections.Generic;
using System.Text;

namespace Sunlighter.CanonicalTypes
{
#if !NETSTANDARD2_0
    public sealed class ObjectIDGenerator
    {
        private readonly object syncRoot;
        private long nextId;
        private readonly Dictionary<object, long> dict;

        public ObjectIDGenerator()
        {
            syncRoot = new object();
            nextId = 0L;
            dict = new Dictionary<object, long>(ReferenceEqualityComparer.Instance);
        }

        public long GetId(object obj, out bool firstTime)
        {
            lock (syncRoot)
            {
                if (dict.TryGetValue(obj, out long id))
                {
                    firstTime = false;
                    return id;
                }
                else
                {
                    long newId = nextId;
                    ++nextId;
                    dict.Add(obj, newId);
                    firstTime = true;
                    return newId;
                }
            }
        }
    }
#endif
}
