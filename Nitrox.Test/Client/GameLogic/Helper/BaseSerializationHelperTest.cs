using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Logger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroxClient.GameLogic.Helper;

[TestClass]
public class BaseSerializationHelperTest
{
    [TestMethod]
    public void TestBytesRestoring()
    {
        List<int> lengths = new() { 10000, 100000, 300000, 500000 };
        foreach (int length in lengths)
        {
            TestSerialization(GenerateRealisticBytes(length));
        }
    }

    [TestMethod]
    public void TestAllZeroBytes()
    {
        List<int> lengths = new() { 10000, 100000, 300000, 500000 };
        foreach (int length in lengths)
        {
            byte[] data = new byte[length];
            TestSerialization(data);
        }
    }

    [TestMethod]
    public void TestAllMaxBytes()
    {
        List<int> lengths = new() { 10000, 100000, 300000, 500000 };
        foreach (int length in lengths)
        {
            byte[] data = new byte[length];
            data.AsSpan().Fill(byte.MaxValue);
            TestSerialization(data);
        }
    }

    /// <summary>
    /// Generates bytes to emulate Base data arrays
    /// </summary>
    private static byte[] GenerateRealisticBytes(int length)
    {
        byte[] generated = new byte[length];
        byte[] randomBytes = new byte[length];
        int randomIndex = 0;
        Random random = new();
        random.NextBytes(randomBytes);
        for (int i = 0; i < length; i++)
        {
            if (random.Next(100) < 3)
            {
                generated[i] = randomBytes[randomIndex++];
            }
            else
            {
                generated[i] = 0;
            }
        }

        return generated;
    }

    private static void TestSerialization(byte[] original)
    {
        byte[] compressed = BaseSerializationHelper.CompressBytes(original);
        byte[] decompressed = BaseSerializationHelper.DecompressBytes(compressed, original.Length);
        Log.Info($"Size: [Original: {original.Length}, Compressed: {compressed.Length}, Decompressed: {decompressed.Length}]");
        Log.Info($"Original: {string.Join(", ", original.Take(100))}");
        Log.Info($"Compressed: {string.Join(", ", compressed.Take(100))}");
        Log.Info($"Decompressed: {string.Join(", ", decompressed.Take(100))}\n");

        Assert.AreEqual(original.Length, decompressed.Length);
        for (int i = 0; i < original.Length; i++)
        {
            Assert.AreEqual(original[i], decompressed[i]);
        }
    }
}
