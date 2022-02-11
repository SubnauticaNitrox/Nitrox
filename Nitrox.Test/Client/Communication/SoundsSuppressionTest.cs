using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using NSubstitute;

namespace Nitrox.Test.Client.Communication;

[TestClass]
public class SoundsSuppressionTest
{
    private readonly PlayFMODAsset testPacket = new("", NitroxVector3.Zero, 0f, 0f, true);
    private readonly PlayFMODCustomLoopingEmitter testPacket2 = new(new NitroxId(), "");
    private readonly NitroxModel.Packets.ToggleLights testPacket3 = new(new NitroxId(), true);

    [TestMethod]
    public void TestSuppressSounds()
    {
        IClient client = Substitute.For<IClient>();
        IMultiplayerSession sessionManager = new MultiplayerSessionManager(client);
        Assert.IsTrue(sessionManager.Send(testPacket));
        using (sessionManager.SuppressSounds())
        {
            Assert.IsFalse(sessionManager.Send(testPacket));
            Assert.IsFalse(sessionManager.Send(testPacket2));
        }
    }

    [TestMethod]
    public void TestUnsuppress()
    {
        IClient client = Substitute.For<IClient>();
        MultiplayerSessionManager sessionManager = new(client);

        using (sessionManager.SuppressSounds())
        {
            using (sessionManager.Unsuppress<PlayFMODAsset>())
            {
                Assert.IsTrue(sessionManager.Send(testPacket));
                Assert.IsFalse(sessionManager.Send(testPacket2));
            }
            Assert.IsFalse(sessionManager.Send(testPacket));
            Assert.IsFalse(sessionManager.Send(testPacket2));
        }
        Assert.IsTrue(sessionManager.suppressedPacketsTypes.Count == 0);
        using (sessionManager.Suppress<NitroxModel.Packets.ToggleLights>())
        {
            using (sessionManager.Unsuppress<NitroxModel.Packets.ToggleLights>())
            {
                Assert.IsTrue(sessionManager.Send(testPacket3));
            }
            Assert.IsFalse(sessionManager.Send(testPacket3));
        }
    }
}
