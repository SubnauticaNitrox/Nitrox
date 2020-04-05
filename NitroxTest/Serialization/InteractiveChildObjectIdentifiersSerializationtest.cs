using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.Serialization;
using NitroxTechType = NitroxModel.DataStructures.TechType;

namespace NitroxTest.Serialization
{
    [TestClass]
    public class InteractiveChildObjectIdentifiersSerializationtest
    {
        private ThreadSafeCollection<InteractiveChildObjectIdentifier> state;
        private readonly Random random = new Random();

        [TestInitialize]
        public void Setup()
        {
            state = new ThreadSafeCollection<InteractiveChildObjectIdentifier>();
            state.Add(new InteractiveChildObjectIdentifier(new NitroxId(), "/BlablaScene/SomeBlablaContainer/BlaItem"));
            byte[] idBytes = new byte[16];
            random.NextBytes(idBytes);
            state.Add(new InteractiveChildObjectIdentifier(new NitroxId(idBytes), ""));
            state.Add(new InteractiveChildObjectIdentifier(new NitroxId(), " herp "));
        }

        [TestMethod]
        public void Sanity()
        {
            ServerProtobufSerializer server = new ServerProtobufSerializer();

            ThreadSafeCollection<InteractiveChildObjectIdentifier> deserialized;
            using (MemoryStream stream = new MemoryStream())
            {
                server.Serialize(stream, state);
                stream.Position = 0;
                deserialized = server.Deserialize<ThreadSafeCollection<InteractiveChildObjectIdentifier>>(stream);
            }

            deserialized.Count.Should().BeGreaterThan(0);
            deserialized[0].GameObjectNamePath.ShouldBeEquivalentTo("/BlablaScene/SomeBlablaContainer/BlaItem");
            deserialized[1].GameObjectNamePath.ShouldBeEquivalentTo("");
            deserialized[2].GameObjectNamePath.ShouldBeEquivalentTo(" herp ");

            int iterationCount = 0;
            foreach (InteractiveChildObjectIdentifier id in deserialized)
            {
                iterationCount++;
                id.Id.ToString().Should().MatchRegex("^[a-zA-Z0-9\\-]{10,}$"); // matches hex and - character
                id.GameObjectNamePath.Should().NotBeNull();
            }
            
            iterationCount.ShouldBeEquivalentTo(3);
        }
    }
}
