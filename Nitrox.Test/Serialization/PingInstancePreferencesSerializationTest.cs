using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Logger;
using NitroxServer.GameLogic.Players;
using NitroxServer.Serialization;

namespace NitroxTest.Serialization
{
    [TestClass]
    public class PingInstancePreferencesSerializationTest
    {
        private PingInstancePreferences preferences;

        [TestInitialize]
        public void Setup()
        {
            preferences = new PingInstancePreferences(new() { "ok", "it", "works" }, new());
            preferences.ColorPreferences["ok"] = 5;
            preferences.ColorPreferences["it"] = 1;
            preferences.ColorPreferences["works"] = 10;
        }

        [TestMethod]
        public void Sanity()
        {
            IServerSerializer serializer = new ServerJsonSerializer();

            PingInstancePreferences deserialized;
            byte[] buffer;
            using (MemoryStream stream = new())
            {
                serializer.Serialize(stream, preferences);
                buffer = stream.GetBuffer();
            }

            using (MemoryStream stream = new(buffer))
            {
                deserialized = serializer.Deserialize<PingInstancePreferences>(stream);
            }

            deserialized.HiddenSignalPings.Count.Should().Be(3);
            deserialized.HiddenSignalPings.Should().Contain("ok", "it", "works");
            deserialized.ColorPreferences.Count.Should().Be(3);
            deserialized.ColorPreferences["ok"].Should().Be(5);
            deserialized.ColorPreferences["it"].Should().Be(1);
            deserialized.ColorPreferences["works"].Should().Be(10);
        }
    }
}
