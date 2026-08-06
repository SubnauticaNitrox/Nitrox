using System.Runtime.Serialization;
using System.Text;
using Microsoft.Extensions.Logging;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.Serialization;
using NSubstitute;

namespace Nitrox.Server.Subnautica.Models.Serialization;

[TestClass]
public sealed class MapRoomEntitySerializationTest
{
    private static readonly NitroxInt3 cell = new(2, -1, 4);
    private static readonly NitroxVector3 scanOrigin = new(125.5f, -30.25f, 87.75f);
    private static readonly NitroxTechType quartz = new("Quartz");

    [TestMethod]
    public void JsonRoundTripPreservesScannerRoomState()
    {
        ServerJsonSerializer serializer = CreateJsonSerializer();
        MapRoomEntity original = CreateMapRoom(scanOrigin, new ScannerRoomScanState(quartz, 17));

        using MemoryStream output = new();
        serializer.Serialize(output, original);
        using MemoryStream input = new(output.ToArray());

        MapRoomEntity deserialized = serializer.Deserialize<MapRoomEntity>(input);

        deserialized.Cell.Should().Be(cell);
        deserialized.ScanOrigin.Should().Be(scanOrigin);
        deserialized.ScanState.SelectedTechType.Should().Be(quartz);
        deserialized.ScanState.Version.Should().Be(17);
    }

    [TestMethod]
    public void JsonWithoutScanOriginDeserializesAsNull()
    {
        ServerJsonSerializer serializer = CreateJsonSerializer();
        const string legacyJson = "{\"Cell\":{\"X\":2,\"Y\":-1,\"Z\":4}}";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(legacyJson));

        MapRoomEntity deserialized = serializer.Deserialize<MapRoomEntity>(stream);

        deserialized.Cell.Should().Be(cell);
        deserialized.ScanOrigin.Should().BeNull();
        deserialized.ScanState.SelectedTechType.Should().BeNull();
        deserialized.ScanState.Version.Should().Be(0);
    }

    [TestMethod]
    public void ProtoBufRoundTripPreservesScannerRoomState()
    {
        MapRoomProtoBufSerializer serializer = new();
        MapRoomEntity original = CreateMapRoom(scanOrigin, new ScannerRoomScanState(quartz, 17));

        using MemoryStream stream = new();
        serializer.Serialize(stream, original);
        stream.Position = 0;
        MapRoomEntity deserialized = CreateMapRoom(null, ScannerRoomScanState.Empty);
        serializer.Deserialize(stream, deserialized, typeof(MapRoomEntity));

        deserialized.Cell.Should().Be(cell);
        deserialized.ScanOrigin.Should().Be(scanOrigin);
        deserialized.ScanState.SelectedTechType.Should().Be(quartz);
        deserialized.ScanState.Version.Should().Be(17);
    }

    [TestMethod]
    public void ProtoBufWithoutScanOriginDeserializesAsNull()
    {
        MapRoomProtoBufSerializer serializer = new();
        MapRoomEntity entityWithoutOrigin = CreateMapRoom(null, ScannerRoomScanState.Empty);

        using MemoryStream stream = new();
        serializer.Serialize(stream, entityWithoutOrigin);
        stream.Position = 0;
        MapRoomEntity deserialized = CreateMapRoom(null, ScannerRoomScanState.Empty);
        serializer.Deserialize(stream, deserialized, typeof(MapRoomEntity));

        deserialized.Cell.Should().Be(cell);
        deserialized.ScanOrigin.Should().BeNull();
    }

    [TestMethod]
    public void ProtoBufWithoutScanStateDeserializesAsEmpty()
    {
        MapRoomProtoBufSerializer serializer = new();
        MapRoomEntity legacyMapRoom = CreateMapRoom(scanOrigin, ScannerRoomScanState.Empty);
        legacyMapRoom.ScanState = null!;
        MapRoomContainer legacyContainer = new() { MapRoom = legacyMapRoom };

        using MemoryStream stream = new();
        serializer.Serialize(stream, legacyContainer);
        stream.Position = 0;
        MapRoomEntity deserialized = serializer.Deserialize<MapRoomContainer>(stream).MapRoom!;

        deserialized.ScanState.Should().NotBeNull();
        deserialized.ScanState.SelectedTechType.Should().BeNull();
        deserialized.ScanState.Version.Should().Be(0);
    }

    private static ServerJsonSerializer CreateJsonSerializer() => new(Substitute.For<ILogger<ServerJsonSerializer>>());

    private static MapRoomEntity CreateMapRoom(NitroxVector3? origin, ScannerRoomScanState scanState) => new(
        new NitroxId("884e85f4-2b46-4239-a33a-9f687338b11a"),
        new NitroxId("049d1ac2-6a77-46b5-8284-f862a085251d"),
        cell,
        origin,
        scanState);

    private sealed class MapRoomProtoBufSerializer : ServerProtoBufSerializer
    {
        public MapRoomProtoBufSerializer() : base(null, "Nitrox.Model", "Nitrox.Model.Subnautica")
        {
            Model.Add(typeof(MapRoomContainer), true);
        }
    }

    [DataContract]
    public sealed class MapRoomContainer
    {
        [DataMember(Order = 1)]
        public MapRoomEntity? MapRoom { get; set; }
    }
}
