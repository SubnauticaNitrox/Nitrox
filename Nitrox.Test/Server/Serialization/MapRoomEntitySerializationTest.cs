using System.Text;
using Microsoft.Extensions.Logging;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Server.Subnautica.Models.Serialization;
using NSubstitute;

namespace Nitrox.Server.Subnautica.Models.Serialization;

[TestClass]
public sealed class MapRoomEntitySerializationTest
{
    private static readonly NitroxInt3 cell = new(2, -1, 4);
    private static readonly NitroxVector3 scanOrigin = new(125.5f, -30.25f, 87.75f);

    [TestMethod]
    public void JsonRoundTripPreservesScanOrigin()
    {
        ServerJsonSerializer serializer = CreateJsonSerializer();
        MapRoomEntity original = CreateMapRoom(scanOrigin);

        using MemoryStream output = new();
        serializer.Serialize(output, original);
        using MemoryStream input = new(output.ToArray());

        MapRoomEntity deserialized = serializer.Deserialize<MapRoomEntity>(input);

        deserialized.Cell.Should().Be(cell);
        deserialized.ScanOrigin.Should().Be(scanOrigin);
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
    }

    [TestMethod]
    public void ProtoBufRoundTripPreservesScanOrigin()
    {
        MapRoomProtoBufSerializer serializer = new();
        MapRoomEntity original = CreateMapRoom(scanOrigin);

        using MemoryStream stream = new();
        serializer.Serialize(stream, original);
        stream.Position = 0;
        MapRoomEntity deserialized = CreateMapRoom(null);
        serializer.Deserialize(stream, deserialized, typeof(MapRoomEntity));

        deserialized.Cell.Should().Be(cell);
        deserialized.ScanOrigin.Should().Be(scanOrigin);
    }

    [TestMethod]
    public void ProtoBufWithoutScanOriginDeserializesAsNull()
    {
        MapRoomProtoBufSerializer serializer = new();
        MapRoomEntity entityWithoutOrigin = CreateMapRoom(null);

        using MemoryStream stream = new();
        serializer.Serialize(stream, entityWithoutOrigin);
        stream.Position = 0;
        MapRoomEntity deserialized = CreateMapRoom(null);
        serializer.Deserialize(stream, deserialized, typeof(MapRoomEntity));

        deserialized.Cell.Should().Be(cell);
        deserialized.ScanOrigin.Should().BeNull();
    }

    private static ServerJsonSerializer CreateJsonSerializer() => new(Substitute.For<ILogger<ServerJsonSerializer>>());

    private static MapRoomEntity CreateMapRoom(NitroxVector3? origin) => new(
        new NitroxId("884e85f4-2b46-4239-a33a-9f687338b11a"),
        new NitroxId("049d1ac2-6a77-46b5-8284-f862a085251d"),
        cell,
        origin);

    private sealed class MapRoomProtoBufSerializer : ServerProtoBufSerializer
    {
        public MapRoomProtoBufSerializer() : base(null, "Nitrox.Model", "Nitrox.Model.Subnautica")
        {
        }
    }
}
