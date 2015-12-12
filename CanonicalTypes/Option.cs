using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes
{
    public class Option<T>
    {
        private bool hasValue;
        private T value;

        private Option(bool hasValue, T value)
        {
            this.hasValue = hasValue;
            this.value = value;
        }

        public bool HasValue => hasValue;

        public T Value
        {
            get
            {
                if (hasValue)
                {
                    return value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public static Option<T> Some(T value)
        {
            return new Option<T>(true, value);
        }

        public static Option<T> None
        {
            get
            {
                return new Option<T>(false, default(T));
            }
        }
    }
}
