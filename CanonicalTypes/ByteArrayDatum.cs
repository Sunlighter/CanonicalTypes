using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunlighter.CanonicalTypes
{
    public class ByteArrayDatum : Datum
    {
        private ImmutableArray<byte> bytes;

        public ByteArrayDatum(ImmutableArray<byte> bytes)
        {
            this.bytes = bytes;
        }

        public static ByteArrayDatum FromByteArray(byte[] bytes)
        {
            var iab = ImmutableArray<byte>.Empty.ToBuilder();
            iab.AddRange(bytes, bytes.Length);
            return new ByteArrayDatum(iab.ToImmutable());
        }

        public ImmutableArray<byte> Bytes { get { return bytes; } }

        public int Length { get { return bytes.Length; } }

        public byte this[int index]
        {
            get { return bytes[index]; }
        }

        public override DatumType DatumType => DatumType.ByteArray;

        public override T Visit<T>(IDatumVisitor<T> visitor) => visitor.VisitByteArray(this);

        public override T Visit<T>(IDatumVisitorWithState<T> visitor, T state) => visitor.VisitByteArray(state, this);
    }
}
