using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nitrox.Model.Extensions;

public static class ByteArrayExtensions
{
    extension(byte[] self)
    {
        /// <summary>
        ///     Returns a new array that has the result of the bitwise operation.
        /// </summary>
        public static byte[] operator &(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(a), $"Array '{nameof(b)}' must have the same length as array '{nameof(a)}'.");
            }

            a = a.CreateCopy();
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = (byte)(a[i] & b[i]);
            }
            return a;
        }

        /// <summary>
        ///     Left shifts the array by the given amount.
        /// </summary>
        public static byte[] operator <<(byte[] array, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Value must not be negative");
            }
            if (amount == 0)
            {
                return array;
            }
            Span<int> span = MemoryMarshal.Cast<byte, int>((Span<byte>)array);
            int lengthFromBitLength;
            int bitLength = array.Length * 8;
            if (amount < bitLength)
            {
                int index1 = (int)((uint)(bitLength - 1) / 32U);
                lengthFromBitLength = Math.DivRem(amount, 32, out int remainder);
                if (remainder == 0)
                {
                    span.Slice(0, index1 + 1 - lengthFromBitLength).CopyTo(span.Slice(lengthFromBitLength));
                }
                else
                {
                    int index2 = index1 - lengthFromBitLength;
                    while (index2 > 0)
                    {
                        int num1 = ReverseIfBigEndian(span[index2]) << remainder;
                        uint num2 = (uint)(ReverseIfBigEndian(span[--index2]) >>> (32 /*0x20*/ - remainder));
                        span[index1] = ReverseIfBigEndian(num1 | (int)num2);
                        --index1;
                    }
                    span[index1] = ReverseIfBigEndian(ReverseIfBigEndian(span[index2]) << remainder);
                }
            }
            else
            {
                lengthFromBitLength = GetInt32ArrayLengthFromBitLength(bitLength);
            }
            span.Slice(0, lengthFromBitLength).Clear();
            return array;

            static int GetInt32ArrayLengthFromBitLength(int bitLength) => (int)(((uint)bitLength + 31u) >> 5);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int ReverseIfBigEndian(int value) => BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value);
        }

        /// <summary>
        ///     Reverses the contents of the array if current machine is not Little Endian.
        /// </summary>
        public void AsNetworkOrder()
        {
            // See: https://en.wikipedia.org/wiki/Endianness
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(self);
            }
        }

        public void Fill(byte value)
        {
            Span<byte> span = self.AsSpan();
            span.Fill(value);
        }

        private byte[] CreateCopy()
        {
            byte[] copy = new byte[self.Length];
            Array.Copy(self, copy, copy.Length);
            return copy;
        }
    }
}
