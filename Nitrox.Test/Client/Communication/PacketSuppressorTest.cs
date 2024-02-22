using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Packets;

namespace NitroxClient.Communication;

[TestClass]
public class PacketSuppressorTest
{
    [TestMethod]
    public void SingleSuppress()
    {
        Assert.IsFalse(PacketSuppressor<BedEnter>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODAssetPacket>.IsSuppressed);

        using (PacketSuppressor<BedEnter>.Suppress())
        {
            Assert.IsTrue(PacketSuppressor<BedEnter>.IsSuppressed);
            Assert.IsFalse(PacketSuppressor<FMODAssetPacket>.IsSuppressed);
        }

        Assert.IsFalse(PacketSuppressor<BedEnter>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODAssetPacket>.IsSuppressed);
    }

    [TestMethod]
    public void MultipleSuppress()
    {
        Assert.IsFalse(PacketSuppressor<BedEnter>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODAssetPacket>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODEventInstancePacket>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODCustomEmitterPacket>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODCustomLoopingEmitterPacket>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODStudioEmitterPacket>.IsSuppressed);

        using (PacketSuppressor<FMODAssetPacket, FMODEventInstancePacket, FMODCustomEmitterPacket, FMODCustomLoopingEmitterPacket, FMODStudioEmitterPacket>.Suppress())
        {
            Assert.IsFalse(PacketSuppressor<BedEnter>.IsSuppressed);

            Assert.IsTrue(PacketSuppressor<FMODAssetPacket>.IsSuppressed);
            Assert.IsTrue(PacketSuppressor<FMODEventInstancePacket>.IsSuppressed);
            Assert.IsTrue(PacketSuppressor<FMODCustomEmitterPacket>.IsSuppressed);
            Assert.IsTrue(PacketSuppressor<FMODCustomLoopingEmitterPacket>.IsSuppressed);
            Assert.IsTrue(PacketSuppressor<FMODStudioEmitterPacket>.IsSuppressed);
        }

        Assert.IsFalse(PacketSuppressor<BedEnter>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODAssetPacket>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODEventInstancePacket>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODCustomEmitterPacket>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODCustomLoopingEmitterPacket>.IsSuppressed);
        Assert.IsFalse(PacketSuppressor<FMODStudioEmitterPacket>.IsSuppressed);
    }
}
