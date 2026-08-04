using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Model.Packets;

[TestClass]
public sealed class ScannerRoomPacketSerializationTest
{
    private readonly NitroxId roomId = new("54d7a539-e157-4e47-9df1-b12d31b82fa5");
    private readonly NitroxTechType quartz = new("Quartz");

    [TestMethod]
    public void QueryRoundTripsEveryField()
    {
        Packet.InitSerializer();
        ScannerRoomQuery packet = new(roomId, 17, 450, quartz, 123456, new NitroxVector3(1, 2, 3));

        ScannerRoomQuery deserialized = Packet.Deserialize(packet.Serialize()).Should().BeOfType<ScannerRoomQuery>().Which;

        deserialized.MapRoomId.Should().Be(roomId);
        deserialized.RequestId.Should().Be(17);
        deserialized.ReportedRange.Should().Be(450);
        deserialized.SelectedTechType.Should().Be(quartz);
        deserialized.KnownRevision.Should().Be(123456);
        deserialized.ObservedOrigin.Should().Be(new NitroxVector3(1, 2, 3));
    }

    [TestMethod]
    public void SnapshotPageRoundTripsEveryField()
    {
        Packet.InitSerializer();
        NitroxId entityId = new("07e8b483-9cca-43b5-aa61-45b51f109881");
        ScannerRoomSnapshotPage packet = new(
            roomId,
            23,
            ScannerRoomQueryStatus.Complete,
            500,
            quartz,
            987654,
            1,
            3,
            [new ScannerResourceSummary(quartz, 513)],
            [new ScannerResourceTarget(entityId, 4, quartz, new NitroxVector3(10, 20, 30))]);

        ScannerRoomSnapshotPage deserialized = Packet.Deserialize(packet.Serialize()).Should().BeOfType<ScannerRoomSnapshotPage>().Which;

        deserialized.MapRoomId.Should().Be(roomId);
        deserialized.RequestId.Should().Be(23);
        deserialized.Status.Should().Be(ScannerRoomQueryStatus.Complete);
        deserialized.EffectiveRange.Should().Be(500);
        deserialized.SelectedTechType.Should().Be(quartz);
        deserialized.Revision.Should().Be(987654);
        deserialized.PageIndex.Should().Be(1);
        deserialized.PageCount.Should().Be(3);
        deserialized.AvailableResources.Should().ContainSingle(summary => summary.TechType.Equals(quartz) && summary.Count == 513);
        deserialized.Targets.Should().ContainSingle(target => target.EntityId == entityId && target.TrackerIndex == 4 && target.Position == new NitroxVector3(10, 20, 30));
    }
}
