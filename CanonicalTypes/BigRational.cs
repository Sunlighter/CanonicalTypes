using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sunlighter.CanonicalTypes
{
    public enum RoundingMode
    {
        Floor,
        Round,
        Ceiling,
        TruncateTowardZero
    }

    public class BigRational
    {
        private BigInteger numerator;
        private BigInteger denominator;

        public BigRational(BigInteger numerator, BigInteger denominator)
        {
            if (denominator.IsZero) throw new ArgumentException("Denominator must be non-zero");

            BigInteger n = numerator;
            BigInteger d = denominator;

            if (d.Sign < 0)
            {
                n = -n;
                d = -d;
            }

            BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(n), BigInteger.Abs(d));

            if (gcd != BigInteger.One)
            {
                n /= gcd;
                d /= gcd;
            }

            this.numerator = n;
            this.denominator = d;
        }

        public bool IsNegative { get { return numerator.Sign < 0; } }
        public bool IsZero { get { return numerator.IsZero; } }

        public BigInteger Numerator { get { return numerator; } }
        public BigInteger Denominator { get { return denominator; } }

        public static BigRational operator +(BigRational a, BigRational b)
        {
            return new BigRational(a.numerator * b.denominator + b.numerator * a.denominator, a.denominator * b.denominator);
        }

        public static BigRational operator -(BigRational a)
        {
            return new BigRational(-a.numerator, a.denominator);
        }

        public static BigRational operator -(BigRational a, BigRational b)
        {
            return new BigRational(a.numerator * b.denominator - b.numerator * a.denominator, a.denominator * b.denominator);
        }

        public static BigRational operator *(BigRational a, BigRational b)
        {
            return new BigRational(a.numerator * b.numerator, a.denominator * b.denominator);
        }

        public static BigRational operator /(BigRational a, BigRational b)
        {
            return new BigRational(a.numerator * b.denominator, a.denominator * b.numerator);
        }

        public static BigRational operator +(BigRational a, BigInteger b)
        {
            return new BigRational(a.numerator + b * a.denominator, a.denominator);
        }

        public static BigRational operator +(BigInteger a, BigRational b)
        {
            return new BigRational(a * b.denominator + b.numerator, b.denominator);
        }

        public static BigRational operator -(BigRational a, BigInteger b)
        {
            return new BigRational(a.numerator - b * a.denominator, a.denominator);
        }

        public static BigRational operator -(BigInteger a, BigRational b)
        {
            return new BigRational(a * b.denominator - b.numerator, b.denominator);
        }

        public static BigRational operator *(BigRational a, BigInteger b)
        {
            return new BigRational(a.numerator * b, a.denominator);
        }

        public static BigRational operator *(BigInteger a, BigRational b)
        {
            return new BigRational(a * b.numerator, b.denominator);
        }

        public static BigRational operator /(BigRational a, BigInteger b)
        {
            return new BigRational(a.numerator, a.denominator * b);
        }

        public static BigRational operator /(BigInteger a, BigRational b)
        {
            return new BigRational(a * b.denominator, b.numerator);
        }

        public static implicit operator BigRational(BigInteger i)
        {
            return new BigRational(i, BigInteger.One);
        }

        public static implicit operator BigRational(int i)
        {
            return new BigRational(i, BigInteger.One);
        }

        public static implicit operator BigRational(long l)
        {
            return new BigRational(l, BigInteger.One);
        }

        public static explicit operator double (BigRational a)
        {
            return a.GetDoubleValue(RoundingMode.Round);
        }

        public static explicit operator float (BigRational a)
        {
            return a.GetSingleValue(RoundingMode.Round);
        }

        public BigInteger Floor()
        {
            System.Diagnostics.Debug.Assert(denominator > BigInteger.Zero);
            BigInteger b = numerator / denominator;
            if (numerator < BigInteger.Zero) b = b - BigInteger.One;
            return b;
        }

        public BigInteger Round()
        {
            System.Diagnostics.Debug.Assert(denominator > BigInteger.Zero);
            BigRational r = this + new BigRational(BigInteger.One, (BigInteger)2);
            if (r.Denominator == BigInteger.One)
            {
                if (!r.Numerator.IsEven)
                {
                    return r.Numerator - BigInteger.One;
                }
                else return r.Numerator;
            }
            else return r.Floor();
        }

        public BigInteger Ceiling()
        {
            System.Diagnostics.Debug.Assert(denominator > BigInteger.Zero);
            BigInteger b = numerator / denominator;
            if (numerator >= BigInteger.Zero) b = b + BigInteger.One;
            return b;
        }

        public BigInteger TruncateTowardZero()
        {
            BigInteger b = numerator / denominator;
            return b;
        }

        public BigInteger RoundingOp(RoundingMode m)
        {
            switch (m)
            {
                case RoundingMode.Ceiling: return Ceiling();
                case RoundingMode.Floor: return Floor();
                case RoundingMode.Round: return Round();
                case RoundingMode.TruncateTowardZero: return TruncateTowardZero();
                default: goto case RoundingMode.Round;
            }
        }

        public static bool operator <(BigRational a, BigRational b)
        {
            return a.numerator * b.denominator < b.numerator * a.denominator;
        }

        public static bool operator >(BigRational a, BigRational b)
        {
            return a.numerator * b.denominator > b.numerator * a.denominator;
        }

        public static bool operator <=(BigRational a, BigRational b)
        {
            return a.numerator * b.denominator <= b.numerator * a.denominator;
        }

        public static bool operator >=(BigRational a, BigRational b)
        {
            return a.numerator * b.denominator >= b.numerator * a.denominator;
        }

        public static bool operator ==(BigRational a, BigRational b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return (a.numerator == b.numerator) && (a.denominator == b.denominator);
        }

        public static bool operator !=(BigRational a, BigRational b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return (a.numerator != b.numerator) || (a.denominator != b.denominator);
        }

        public override bool Equals(object obj)
        {
            return (obj is BigRational) && (this == (BigRational)obj);
        }

        public override int GetHashCode()
        {
            return $"{numerator}/{denominator}".GetHashCode();
        }

        public static BigRational Min(BigRational a, BigRational b)
        {
            return (a < b) ? a : b;
        }

        public static BigRational Max(BigRational a, BigRational b)
        {
            return (a > b) ? a : b;
        }

        public BigRational Reciprocal()
        {
            return new BigRational(denominator, numerator);
        }

        public static BigRational Gcd(BigRational a, BigRational b)
        {
            return new BigRational
            (
                BigInteger.GreatestCommonDivisor(a.Numerator * b.Denominator, b.Numerator * a.Denominator),
                a.Denominator * b.Denominator
            );
        }

        public static BigInteger Lcm(BigInteger a, BigInteger b)
        {
            return a * (b / BigInteger.GreatestCommonDivisor(a, b));
        }

        public static BigRational Lcm(BigRational a, BigRational b)
        {
            return new BigRational
            (
                Lcm(a.Numerator * b.Denominator, b.Numerator * a.Denominator),
                a.Denominator * b.Denominator
            );
        }
        
        public static BigRational Pow(BigRational @base, int expt)
        {
            if (expt < 0) return Pow(@base.Reciprocal(), -expt);
            return new BigRational(BigInteger.Pow(@base.Numerator, expt), BigInteger.Pow(@base.Denominator, expt));
        }

        private static Lazy<BigRational> zero = new Lazy<BigRational>(() => new BigRational(0, 1), LazyThreadSafetyMode.ExecutionAndPublication);
        public static BigRational Zero { get { return zero.Value; } }

        private static Lazy<BigRational> one = new Lazy<BigRational>(() => new BigRational(1, 1), LazyThreadSafetyMode.ExecutionAndPublication);
        public static BigRational One { get { return one.Value; } }

        private static Lazy<BigRational> two = new Lazy<BigRational>(() => new BigRational(2, 1), LazyThreadSafetyMode.ExecutionAndPublication);
        public static BigRational Two { get { return two.Value; } }

        private static Lazy<BigRational> oneHalf = new Lazy<BigRational>(() => new BigRational(1, 2), LazyThreadSafetyMode.ExecutionAndPublication);
        public static BigRational OneHalf { get { return oneHalf.Value; } }

        private static Lazy<BigRational> minusOne = new Lazy<BigRational>(() => new BigRational(-1, 1), LazyThreadSafetyMode.ExecutionAndPublication);
        public static BigRational MinusOne { get { return minusOne.Value; } }

        public static Tuple<BigRational, int> Normalize(BigRational r)
        {
            if (r.IsNegative)
            {
                Tuple<BigRational, int> result = Normalize(-r);
                return new Tuple<BigRational, int>(-result.Item1, result.Item2);
            }

            Stack<BigRational> powers = new Stack<BigRational>();
            Stack<int> exponents = new Stack<int>();

            BigRational currentPower = null;
            int currentExponent = 0;

            int finalExponent = 0;

            if (r < BigRational.One)
            {
                currentPower = BigRational.OneHalf;
                currentExponent = -1;

                while (r < currentPower)
                {
                    powers.Push(currentPower);
                    exponents.Push(currentExponent);
                    currentPower *= currentPower;
                    currentExponent *= 2;
                }

                while (powers.Count > 0)
                {
                    currentPower = powers.Pop();
                    currentExponent = exponents.Pop();
                    if (r < currentPower)
                    {
                        r /= currentPower;
                        finalExponent += currentExponent;
                    }
                }
            }
            else
            {
                currentPower = BigRational.Two;
                currentExponent = 1;

                while (r > currentPower)
                {
                    powers.Push(currentPower);
                    exponents.Push(currentExponent);
                    currentPower *= currentPower;
                    currentExponent *= 2;
                }

                while (powers.Count > 0)
                {
                    currentPower = powers.Pop();
                    currentExponent = exponents.Pop();
                    if (r > currentPower)
                    {
                        r /= currentPower;
                        finalExponent += currentExponent;
                    }
                }
            }

            while (r >= BigRational.Two)
            {
                r /= BigRational.Two;
                finalExponent += 1;
            }

            while (r < BigRational.One)
            {
                r *= BigRational.Two;
                finalExponent -= 1;
            }

            return new Tuple<BigRational, int>(r, finalExponent);
        }

        private static BigRational doubleFractionScale = new BigRational(0x10000000000000L, BigInteger.One);

        private static long GetInt64Value_Saturate(BigInteger b)
        {
            if (b < long.MinValue)
            {
                return long.MinValue;
            }
            else if (b > long.MaxValue)
            {
                return long.MaxValue;
            }
            else return (long)b;
        }

        private static int GetInt32Value_Saturate(BigInteger b)
        {
            if (b < int.MinValue)
            {
                return int.MinValue;
            }
            else if (b > int.MaxValue)
            {
                return int.MaxValue;
            }
            else return (int)b;
        }

        public double GetDoubleValue(RoundingMode m)
        {
            if (this.IsZero) return 0.0;
            Tuple<BigRational, int> normalized = Normalize(this);
            BigRational frac = normalized.Item1;
            int expt = normalized.Item2;

            expt += 1023;
            int loops = 53;
            while (expt < 0 && loops > 0)
            {
                frac /= BigRational.Two;
                expt += 1;
                loops -= 1;
            }

            if (expt <= 0) { expt = 0; frac /= BigRational.Two; }

            if (expt > 2046) return (frac < BigRational.Zero) ? double.NegativeInfinity : double.PositiveInfinity;

            long bits = GetInt64Value_Saturate((frac * doubleFractionScale).RoundingOp(m));
            if (bits < 0) bits = (-bits) | unchecked((long)0x8000000000000000L);
            bits &= unchecked((long)0x800FFFFFFFFFFFFFL);
            bits |= (long)expt << 52;

            return BitConverter.Int64BitsToDouble(bits);
        }

        private static BigRational singleFractionScale = new BigRational(0x800000, BigInteger.One);

        public float GetSingleValue(RoundingMode m)
        {
            if (this.IsZero) return 0.0f;
            Tuple<BigRational, int> normalized = Normalize(this);
            BigRational frac = normalized.Item1;
            int expt = normalized.Item2;

            expt += 127;
            int loops = 24;
            while (expt < 0 && loops > 0)
            {
                frac /= BigRational.Two;
                expt += 1;
                loops -= 1;
            }

            if (expt <= 0) { expt = 0; frac /= BigRational.Two; }

            if (expt > 254) return (frac < BigRational.Zero) ? float.NegativeInfinity : float.PositiveInfinity;

            int bits = GetInt32Value_Saturate((frac * singleFractionScale).RoundingOp(m));
            if (bits < 0) bits = (-bits) | unchecked((int)0x80000000L);
            bits &= unchecked((int)0x807FFFFFL);
            bits |= expt << 23;

            return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
        }

        private static void DecomposeSingle(float f, out bool isNegative, out int exponent, out int fraction)
        {
            int i = BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
            fraction = i & 0x7FFFFF;
            exponent = ((i >> 23) & 0xFF) - 127;
            if (exponent == 128) throw new ArithmeticException("Float not representable as a rational");
            if (exponent == -127) { exponent = -126; } else { fraction |= 0x800000; }
            isNegative = ((i >> 31) != 0);
        }

        private static void DecomposeDouble(double d, out bool isNegative, out long exponent, out long fraction)
        {
            long l = BitConverter.DoubleToInt64Bits(d);
            fraction = l & 0xFFFFFFFFFFFFFL;
            exponent = ((l >> 52) & 0x7FFL) - 1023;
            if (exponent == 1024) throw new ArithmeticException("Double not representable as a rational");
            if (exponent == -1023) { exponent = -1022; } else { fraction |= 0x10000000000000L; }
            isNegative = ((l >> 63) != 0);
        }

        public static explicit operator BigRational(float f)
        {
            bool isNegative;
            int exponent;
            int fraction;
            DecomposeSingle(f, out isNegative, out exponent, out fraction);
            BigRational val0 = Pow(BigRational.Two, exponent - 23) * new BigRational(fraction, BigInteger.One);
            return isNegative ? -val0 : val0;
        }

        public static explicit operator BigRational(double d)
        {
            bool isNegative;
            long exponent;
            long fraction;
            DecomposeDouble(d, out isNegative, out exponent, out fraction);
            BigRational val0 = Pow(BigRational.Two, unchecked((int)(exponent - 52))) * new BigRational(fraction, BigInteger.One);
            return isNegative ? -val0 : val0;
        }

        public int Sign
        {
            get
            {
                return numerator.Sign;
            }
        }

        public override string ToString()
        {
            return $"{numerator}/{denominator}";
        }
    }
}
