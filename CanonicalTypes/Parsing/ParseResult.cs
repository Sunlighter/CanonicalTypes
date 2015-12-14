using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes.Parsing
{


    public abstract class ParseResult<V>
    {
        public abstract T Visit<T>
        (
            Func<ParseSuccess<V>, T> onSuccess,
            Func<ParseFailure<V>, T> onFailure
        );
    }

    public class ParseSuccess<V> : ParseResult<V>
    {
        public ParseSuccess(int position, int length, V value)
        {
            this.Position = position;
            this.Length = length;
            this.Value = value;
        }

        public int Position { get; }

        public int Length { get; }

        public V Value { get; }

        public override T Visit<T>(Func<ParseSuccess<V>, T> onSuccess, Func<ParseFailure<V>, T> onFailure)
        {
            return onSuccess(this);
        }
    }

    public struct FailureData : IEquatable<FailureData>
    {
        private int position;
        private string message;

        public FailureData(int position, string message)
        {
            this.position = position;
            this.message = message;
        }

        public int Position => position;

        public string Message => message;

        public override bool Equals(object obj)
        {
            if (obj is FailureData)
            {
                FailureData other = (FailureData)obj;
                return position == other.position && message == other.message;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return ("" + position + ":" + message.Quoted()).GetHashCode();
        }

        public override string ToString()
        {
            return "" + position + ": " + message.Quoted();
        }

        public bool Equals(FailureData other)
        {
            return position == other.position && message == other.message;
        }

        public static bool operator == (FailureData x, FailureData y)
        {
            return x.position == y.position && x.message == y.message;
        }

        public static bool operator != (FailureData x, FailureData y) => !(x == y);
    }

    public class ParseFailure<V> : ParseResult<V>
    {
        public ParseFailure(int position, string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Errors = ImmutableList<FailureData>.Empty.Add(new FailureData(position, message));
        }

        public ParseFailure(ImmutableList<FailureData> failureData)
        {
            if (failureData == null) throw new ArgumentNullException(nameof(failureData));

            Errors = failureData;
        }

        public ImmutableList<FailureData> Errors { get; }

        public override T Visit<T>(Func<ParseSuccess<V>, T> onSuccess, Func<ParseFailure<V>, T> onFailure)
        {
            return onFailure(this);
        }
    }

    public static partial class Utility
    {
        private class ParseResultEqualityComparer<T> : IEqualityComparer<ParseResult<T>>
        {
            private readonly IEqualityComparer<T> resultComparer;

            public ParseResultEqualityComparer(IEqualityComparer<T> resultComparer)
            {
                this.resultComparer = resultComparer;
            }

            public bool Equals(ParseResult<T> x, ParseResult<T> y)
            {
                if (x is ParseSuccess<T>)
                {
                    ParseSuccess<T> xs = (ParseSuccess<T>)x;
                    if (y is ParseSuccess<T>)
                    {
                        ParseSuccess<T> ys = (ParseSuccess<T>)y;
                        return xs.Position == ys.Position && xs.Length == ys.Length && resultComparer.Equals(xs.Value, ys.Value);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    ParseFailure<T> xf = (ParseFailure<T>)x;
                    if (y is ParseSuccess<T>)
                    {
                        return false;
                    }
                    else
                    {
                        ParseFailure<T> yf = (ParseFailure<T>)y;
                        return xf.Errors.Count == yf.Errors.Count && Enumerable.Range(0, xf.Errors.Count).All(i => xf.Errors[i] == yf.Errors[i]);
                    }
                }
            }

            public int GetHashCode(ParseResult<T> obj)
            {
                if (obj is ParseSuccess<T>)
                {
                    ParseSuccess<T> ps = (ParseSuccess<T>)obj;
                    return $"{ps.Position},{ps.Length},{resultComparer.GetHashCode(ps.Value)}".GetHashCode();
                }
                else
                {
                    ParseFailure<T> pf = (ParseFailure<T>)obj;
                    return string.Join(", ", pf.Errors.Select(f => f.GetHashCode())).GetHashCode();
                }
            }
        }

        public static IEqualityComparer<ParseResult<T>> GetParseResultEqualityComparer<T>(IEqualityComparer<T> resultComparer)
        {
            return new ParseResultEqualityComparer<T>(resultComparer);
        }

        public static Func<ParseResult<T>, string> GetParseResultStringConverter<T>(Func<T, string> resultToString)
        {
            Func<ParseResult<T>, string> toString = delegate (ParseResult<T> pr)
            {
                if (pr is ParseSuccess<T>)
                {
                    ParseSuccess<T> ps = (ParseSuccess<T>)pr;
                    return $"{{ success, pos = {ps.Position}, len = {ps.Length}, value = {resultToString(ps.Value)} }}";
                }
                else
                {
                    ParseFailure<T> pf = (ParseFailure<T>)pr;
                    return $"{{ failure, {string.Join(",", pf.Errors.Select(f => $"{{ pos = {f.Position}, message = {f.Message.Quoted()} }}"))} }}";

                }
            };

            return toString;
        }

        public static Func<ImmutableList<T>, string> GetImmutableListStringConverter<T>(Func<T, string> itemToString)
        {
            Func<ImmutableList<T>, string> toString = delegate (ImmutableList<T> list)
            {
                if (list == null)
                {
                    return "null";
                }
                else if (list.Count == 0)
                {
                    return "[ ]";
                }
                else
                {
                    return "[ " + string.Join(", ", list.Select(i => itemToString(i))) + " ]";
                }
            };

            return toString;
        }
    }
}
