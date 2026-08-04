using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace NitroxClient.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomVirtualResourceCacheTest
{
    private readonly NitroxId mapRoomId = new("10000000-0000-0000-0000-000000000001");
    private readonly NitroxId entityId = new("20000000-0000-0000-0000-000000000002");
    private readonly NitroxTechType quartz = new("Quartz");

    [TestMethod]
    public void CreatesStableRoomNamespacedIds()
    {
        string first = ScannerRoomVirtualResourceCache<VirtualResource>.CreateUniqueId(mapRoomId, entityId, 7);
        string second = ScannerRoomVirtualResourceCache<VirtualResource>.CreateUniqueId(mapRoomId, entityId, 7);
        NitroxId otherRoomId = new("30000000-0000-0000-0000-000000000003");

        first.Should().Be("nitrox-scanner:10000000-0000-0000-0000-000000000001:20000000-0000-0000-0000-000000000002:7");
        second.Should().Be(first);
        ScannerRoomVirtualResourceCache<VirtualResource>.CreateUniqueId(otherRoomId, entityId, 7).Should().NotBe(first);
    }

    [TestMethod]
    public void DeduplicatesTargetsAndReusesResourcesAcrossSnapshots()
    {
        ScannerRoomVirtualResourceCache<VirtualResource> cache = new(mapRoomId, uniqueId => new VirtualResource(uniqueId));
        ScannerResourceTarget target = Target(7, new NitroxVector3(1, 2, 3));

        IReadOnlyList<ScannerRoomVirtualResource<VirtualResource>> first = cache.Resolve([target, target]);
        IReadOnlyList<ScannerRoomVirtualResource<VirtualResource>> second = cache.Resolve([Target(7, new NitroxVector3(4, 5, 6))]);

        first.Should().ContainSingle();
        second.Should().ContainSingle();
        second[0].Resource.Should().BeSameAs(first[0].Resource);
        second[0].Target.Position.Should().Be(new NitroxVector3(4, 5, 6));
    }

    [TestMethod]
    public void FreshShadowResourceDoesNotMutatePublishedGeneration()
    {
        ScannerRoomVirtualResourceCache<MutableVirtualResource> cache = new(mapRoomId, uniqueId => new MutableVirtualResource(uniqueId));
        ScannerRoomVirtualResource<MutableVirtualResource> published = cache.GetOrCreate(Target(7, new NitroxVector3(1, 2, 3)));
        published.Resource.Position = published.Target.Position;

        ScannerRoomVirtualResource<MutableVirtualResource> shadow = cache.CreateFresh(Target(7, new NitroxVector3(4, 5, 6)));
        shadow.Resource.Position = shadow.Target.Position;

        shadow.Resource.Should().NotBeSameAs(published.Resource);
        shadow.Resource.UniqueId.Should().Be(published.Resource.UniqueId);
        published.Resource.Position.Should().Be(new NitroxVector3(1, 2, 3));
        shadow.Resource.Position.Should().Be(new NitroxVector3(4, 5, 6));
    }

    private ScannerResourceTarget Target(ushort trackerIndex, NitroxVector3 position) =>
        new(entityId, trackerIndex, quartz, position);

    private sealed record VirtualResource(string UniqueId);

    private sealed class MutableVirtualResource(string uniqueId)
    {
        public string UniqueId { get; } = uniqueId;
        public NitroxVector3 Position { get; set; }
    }
}
