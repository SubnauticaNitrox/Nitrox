using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.Serialization;
using NitroxServer.Serialization.Resources;

namespace NitroxTest.Serialization;

[TestClass]
public class InteractiveChildObjectIdentifiersSerializationTest
{
    private ThreadSafeList<InteractiveChildObjectIdentifier> state;
    private readonly Random random = new();

    [TestInitialize]
    public void Setup()
    {
        state = new ThreadSafeList<InteractiveChildObjectIdentifier> { new(new NitroxId(), "/BlablaScene/SomeBlablaContainer/BlaItem") };
        byte[] idBytes = new byte[16];
        random.NextBytes(idBytes);
        state.Add(new InteractiveChildObjectIdentifier(new NitroxId(idBytes), ""));
        state.Add(new InteractiveChildObjectIdentifier(new NitroxId(), " herp "));
    }

    [TestMethod]
    public void SanityJson()
    {
        ServerJsonSerializer serializer = new();

        ThreadSafeList<InteractiveChildObjectIdentifier> deserialized;
        byte[] buffer;
        using (MemoryStream stream = new())
        {
            serializer.Serialize(stream, state);
            buffer = stream.GetBuffer();
        }

        using (MemoryStream stream = new(buffer))
        {
            deserialized = serializer.Deserialize<ThreadSafeList<InteractiveChildObjectIdentifier>>(stream);
        }
            
        TestDeserializedData(deserialized);
    }

    [TestMethod]
    public void SanityProtoBuf()
    {
        ProtoBufCellParser serializer = new();

        ThreadSafeList<InteractiveChildObjectIdentifier> deserialized;
        byte[] buffer;
        using (MemoryStream stream = new())
        {
            serializer.Serialize(stream, state);
            buffer = stream.GetBuffer();
        }

        using (MemoryStream stream = new(buffer))
        {
            deserialized = serializer.Deserialize<ThreadSafeList<InteractiveChildObjectIdentifier>>(stream);
        }

        TestDeserializedData(deserialized);
    }

    private static void TestDeserializedData(ThreadSafeList<InteractiveChildObjectIdentifier> deserialized)
    {
        deserialized.Count.Should().BeGreaterThan(0);
        deserialized[0].GameObjectNamePath.Should().Be("/BlablaScene/SomeBlablaContainer/BlaItem");
        deserialized[1].GameObjectNamePath.Should().Be("");
        deserialized[2].GameObjectNamePath.Should().Be(" herp ");

        int iterationCount = 0;
        foreach (InteractiveChildObjectIdentifier id in deserialized)
        {
            iterationCount++;
            id.Id.ToString().Should().MatchRegex("^[a-zA-Z0-9\\-]{10,}$"); // matches hex and - character
            id.GameObjectNamePath.Should().NotBeNull();
        }

        iterationCount.Should().Be(3);
    }
}
