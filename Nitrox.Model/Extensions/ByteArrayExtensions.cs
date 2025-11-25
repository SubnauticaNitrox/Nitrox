using System;
using System.Buffers.Binary;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nitrox.Model.Extensions;

public static class ByteArrayExtensions
{
    public static byte[] AsNetworkOrder(this byte[] array)
    {
        // See: https://en.wikipedia.org/wiki/Endianness
        if (BitConverter.IsLittleEndian)
        {
            return array.Reverse().ToArray();
        }
        return array;
    }

    public static byte[] BitwiseLeftShift(this byte[] data, int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Value must not be negative");
        }
        if (count == 0)
        {
            return data;
        }
        Span<int> span = MemoryMarshal.Cast<byte, int>((Span<byte>)data);
        int lengthFromBitLength;
        int bitLength = data.Length * 8;
        if (count < bitLength)
        {
            int index1 = (int)((uint)(bitLength - 1) / 32U);
            lengthFromBitLength = Math.DivRem(count, 32, out int remainder);
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
        return data;

        static int GetInt32ArrayLengthFromBitLength(int bitLength) => (int)(((uint)bitLength + 31u) >> 5);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int ReverseIfBigEndian(int value) => BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value);
    }

    public static byte[] CreateCopy(this byte[] array)
    {
        byte[] copy = new byte[array.Length];
        Array.Copy(array, copy, copy.Length);
        return copy;
    }

    public static byte[] BitwiseAnd(this byte[] a, byte[] b)
    {
        int shortestLength = Math.Min(a.Length, b.Length);
        for (int i = 0; i < shortestLength; i++)
        {
            a[i] = (byte)(a[i] & b[i]);
        }
        return a;
    }
}
