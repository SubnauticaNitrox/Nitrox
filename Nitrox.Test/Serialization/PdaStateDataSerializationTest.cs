using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.Serialization;

namespace NitroxTest.Serialization
{
    [TestClass]
    public class PdaStateDataSerializationTest
    {
        private PDAStateData state;

        [TestInitialize]
        public void Setup()
        {
            state = new PDAStateData();
            state.PdaLog.Add(new PDALogEntry("Some key", 25.125f));
            state.EncyclopediaEntries.Add("Test encyclopedia entry");
            state.EncyclopediaEntries.Add("Test encyclopedia entry again");
            state.EncyclopediaEntries.Add("");
            state.PartiallyUnlockedByTechType.Add(new NitroxTechType("Battery"), new PDAEntry(new NitroxTechType("Areogel"), 1f, 1));
        }

        [TestMethod]
        public void Sanity()
        {
            IServerSerializer[] serializers = { new ServerProtoBufSerializer(), new ServerJsonSerializer() };

            foreach (IServerSerializer serializer in serializers)
            {
                PDAStateData deserialized;
                byte[] buffer;
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, state);
                    buffer = stream.GetBuffer();
                }

                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    deserialized = serializer.Deserialize<PDAStateData>(stream);
                }

                deserialized.PdaLog.Count.Should().Be(1);
                deserialized.PdaLog[0].Key.Should().Be("Some key");
                deserialized.EncyclopediaEntries.Count.Should().Be(3);
                deserialized.EncyclopediaEntries[2].Should().Be("");
                deserialized.PartiallyUnlockedByTechType.Count.Should().Be(1);
                deserialized.PartiallyUnlockedByTechType[new NitroxTechType("Battery")].Progress.Should().Be(1f);
            }
        }
    }
}
