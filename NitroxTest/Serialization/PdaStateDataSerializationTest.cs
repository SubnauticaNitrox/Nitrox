using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.Serialization;
using NitroxTechType = NitroxModel.DataStructures.TechType;

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
            ServerProtobufSerializer server = new ServerProtobufSerializer();

            PDAStateData deserialized;
            using (MemoryStream stream = new MemoryStream())
            {
                server.Serialize(stream, state);
                stream.Position = 0;
                deserialized = server.Deserialize<PDAStateData>(stream);
            }

            deserialized.PdaLog.Count.ShouldBeEquivalentTo(1);
            deserialized.PdaLog[0].Key.ShouldBeEquivalentTo("Some key");
            deserialized.EncyclopediaEntries.Count.ShouldBeEquivalentTo(3);
            deserialized.EncyclopediaEntries[2].ShouldBeEquivalentTo("");
            deserialized.PartiallyUnlockedByTechType.Count.ShouldBeEquivalentTo(1);
            deserialized.PartiallyUnlockedByTechType[new NitroxTechType("Battery")].Progress.ShouldBeEquivalentTo(1f);
        }
    }
}
