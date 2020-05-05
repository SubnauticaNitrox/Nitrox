using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.Serialization;

namespace NitroxTest.Serialization
{
    [TestClass]
    public class InteractiveChildObjectIdentifiersSerializationTest
    {
        private ThreadSafeCollection<InteractiveChildObjectIdentifier> state;
        private readonly Random random = new Random();

        [TestInitialize]
        public void Setup()
        {
            state = new ThreadSafeCollection<InteractiveChildObjectIdentifier> { new InteractiveChildObjectIdentifier(new NitroxId(), "/BlablaScene/SomeBlablaContainer/BlaItem") };
            byte[] idBytes = new byte[16];
            random.NextBytes(idBytes);
            state.Add(new InteractiveChildObjectIdentifier(new NitroxId(idBytes), ""));
            state.Add(new InteractiveChildObjectIdentifier(new NitroxId(), " herp "));
        }

        [TestMethod]
        public void Sanity()
        {
            IServerSerializer[] serializers = { new ServerProtoBufSerializer(), new ServerJsonSerializer() };

            foreach (IServerSerializer serializer in serializers)
            {

                ThreadSafeCollection<InteractiveChildObjectIdentifier> deserialized;
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, state);
                    stream.Position = 0;
                    deserialized = serializer.Deserialize<ThreadSafeCollection<InteractiveChildObjectIdentifier>>(stream);
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
}
