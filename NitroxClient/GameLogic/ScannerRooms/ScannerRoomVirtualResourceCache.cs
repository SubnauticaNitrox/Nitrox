using System;
using System.Collections.Generic;
using System.Globalization;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace NitroxClient.GameLogic.ScannerRooms;

internal readonly record struct ScannerRoomVirtualResourceKey(NitroxId EntityId, ushort TrackerIndex);

internal readonly record struct ScannerRoomVirtualResource<TResource>(TResource Resource, ScannerResourceTarget Target);

/// <summary>
/// Keeps room-owned virtual scanner resources stable across snapshots. Resources are deliberately not registered in
/// the global <c>ResourceTrackerDatabase</c>.
/// </summary>
internal sealed class ScannerRoomVirtualResourceCache<TResource>(NitroxId mapRoomId, Func<string, TResource> createResource) where TResource : class
{
    private readonly Dictionary<ScannerRoomVirtualResourceKey, TResource> resourcesByKey = [];

    public IReadOnlyList<ScannerRoomVirtualResource<TResource>> Resolve(IReadOnlyList<ScannerResourceTarget> targets)
    {
        HashSet<ScannerRoomVirtualResourceKey> includedKeys = [];
        List<ScannerRoomVirtualResource<TResource>> resources = new(targets.Count);

        foreach (ScannerResourceTarget target in targets)
        {
            ScannerRoomVirtualResourceKey key = new(target.EntityId, target.TrackerIndex);
            if (!includedKeys.Add(key))
            {
                continue;
            }

            if (!resourcesByKey.TryGetValue(key, out TResource resource))
            {
                resource = createResource(CreateUniqueId(mapRoomId, key.EntityId, key.TrackerIndex));
                resourcesByKey.Add(key, resource);
            }

            resources.Add(new ScannerRoomVirtualResource<TResource>(resource, target));
        }

        return resources;
    }

    internal static string CreateUniqueId(NitroxId mapRoomId, NitroxId entityId, ushort trackerIndex) =>
        string.Concat(
            "nitrox-scanner:",
            mapRoomId.ToString(),
            ":",
            entityId.ToString(),
            ":",
            trackerIndex.ToString(CultureInfo.InvariantCulture));
}
