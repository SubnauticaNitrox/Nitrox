using System;
using System.IO;
using System.IO.Compression;

namespace NitroxClient.GameLogic.Helper;

public static class BaseSerializationHelper
{
    public static byte[] CompressBytes(byte[] array)
    {
        if (array == null)
        {
            return null;
        }
        using MemoryStream output = new();
        using DeflateStream stream = new(output, CompressionLevel.Optimal);
        CompressStream(stream, array);
        stream.Close();
        return output.ToArray();
    }

    public static byte[] DecompressBytes(byte[] array, int size)
    {
        if (array == null)
        {
            return null;
        }
        using MemoryStream input = new(array);
        using DeflateStream stream = new(input, CompressionMode.Decompress);
        return DecompressStream(stream, size);
    }

    public static void CompressStream(Stream stream, byte[] array)
    {
        using BinaryWriter writer = new(stream);

        ushort zeroCounter = 0;
        foreach (byte value in array)
        {
            if (value == 0 && zeroCounter != ushort.MaxValue)
            {
                zeroCounter++;
            }
            else
            {
                writer.Write(zeroCounter);
                writer.Write(value);
                zeroCounter = 0;
            }
        }

        if (zeroCounter != 0)
        {
            writer.Write(zeroCounter);
        }

        writer.Close();
    }

    public static byte[] DecompressStream(Stream stream, int size)
    {
        using BinaryReader reader = new(stream);
        byte[] result = new byte[size];

        int i = 0;
        bool zeroPart = true;
        while (i < size)
        {
            if (zeroPart)
            {
                ushort zeroLength = reader.ReadUInt16();

                for (int c = 0; c < zeroLength; c++)
                {
                    result[i] = 0;
                    i++;
                }
            }
            else
            {
                result[i] = reader.ReadByte();
                i++;
            }

            zeroPart = !zeroPart;
        }

        return result;
    }

    public static byte[] CompressData<TInput>(TInput[] array, Converter<TInput, byte> converter)
    {
        return CompressBytes(Array.ConvertAll(array, converter));
    }

    public static TInput[] DecompressData<TInput>(byte[] array, int size, Converter<byte, TInput> converter)
    {
        if (array == null)
        {
            return null;
        }
        return Array.ConvertAll(DecompressBytes(array, size), converter);
    }
}
