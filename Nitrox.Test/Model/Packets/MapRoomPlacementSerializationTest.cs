using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Bases;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Model.Packets;

[TestClass]
public sealed class MapRoomPlacementSerializationTest
{
    [TestMethod]
    public void UpdateBasePacketPreservesMapRoomPlacement()
    {
        Packet.InitSerializer();

        NitroxId mapRoomId = new("130d8f94-138d-4b30-a77f-1cd991094a67");
        NitroxVector3 scanOrigin = new(12.5f, -20.25f, 44.75f);
        UpdateBase packet = new(
            new NitroxId("9ff1e85e-33bc-4478-aa04-856e475f473b"),
            new NitroxId("3285c86a-a2bd-4a33-b898-20584cb1205d"),
            null,
            null,
            new(),
            new(),
            new Dictionary<NitroxId, MapRoomPlacement>
            {
                [mapRoomId] = new(new NitroxInt3(3, 4, 5), scanOrigin)
            },
            (null, null),
            7);

        UpdateBase deserialized = Packet.Deserialize(packet.Serialize()).Should().BeOfType<UpdateBase>().Which;

        deserialized.UpdatedMapRooms.Should().ContainKey(mapRoomId);
        deserialized.UpdatedMapRooms[mapRoomId].Should().Be(new MapRoomPlacement(new NitroxInt3(3, 4, 5), scanOrigin));
    }

    [TestMethod]
    public void UpdateBasePacketPreservesMissingMapRoomOrigin()
    {
        Packet.InitSerializer();

        NitroxId mapRoomId = new("ad77a729-c394-4d49-9472-01beae328413");
        UpdateBase packet = new(
            new NitroxId("c0d58417-c766-4024-978d-81f98c8c583f"),
            new NitroxId("5d29c337-907f-41c2-a4cb-48a260ee09d1"),
            null,
            null,
            new(),
            new(),
            new Dictionary<NitroxId, MapRoomPlacement>
            {
                [mapRoomId] = new(new NitroxInt3(-2, 1, 8), null)
            },
            (null, null),
            3);

        UpdateBase deserialized = Packet.Deserialize(packet.Serialize()).Should().BeOfType<UpdateBase>().Which;

        deserialized.UpdatedMapRooms[mapRoomId].ScanOrigin.Should().BeNull();
    }
}
