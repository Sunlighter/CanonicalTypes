using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunlighter.CanonicalTypes
{
    public class JoinResult<K, L, R>
    {
        private K key;
        private L left;
        private R right;

        public JoinResult(K key, L left, R right)
        {
            this.key = key;
            this.left = left;
            this.right = right;
        }

        public K Key { get { return key; } }

        public L Left { get { return left; } }

        public R Right { get { return right; } }
    }

    public enum Nothing
    {
        Value,
    }
}
