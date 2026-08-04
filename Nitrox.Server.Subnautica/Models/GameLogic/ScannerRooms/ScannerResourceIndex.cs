using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Extensions;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.Helper;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal sealed class ScannerResourceIndex(IScannerRoomResourceCatalog resourceCatalog) : IWorldEntityLifecycleObserver
{
    private readonly IScannerRoomResourceCatalog resourceCatalog = resourceCatalog;
    private readonly Lock indexLock = new();
    private readonly Dictionary<NitroxInt3, Dictionary<ScannerResourceNodeKey, ScannerResourceNode>> nodesByBatch = [];
    private readonly Dictionary<NitroxId, List<(NitroxInt3 BatchId, ScannerResourceNodeKey Key)>> keysByEntity = [];
    private long revision;

    public long Revision => Interlocked.Read(ref revision);

    public void EntityTracked(WorldEntity entity) => AddOrReplace(entity);

    public void EntityMoved(WorldEntity entity) => AddOrReplace(entity);

    public void EntityUntracked(WorldEntity entity)
    {
        lock (indexLock)
        {
            if (RemoveEntityUnsafe(entity.Id))
            {
                Interlocked.Increment(ref revision);
            }
        }
    }

    public IReadOnlyList<ScannerResourceNode> Query(IReadOnlyCollection<NitroxInt3> batchIds, NitroxVector3 center, float radius)
    {
        HashSet<NitroxInt3> requestedBatches = batchIds as HashSet<NitroxInt3> ?? [.. batchIds];
        List<ScannerResourceNode> result = [];
        float radiusSquared = radius * radius;

        lock (indexLock)
        {
            foreach (NitroxInt3 batchId in requestedBatches)
            {
                if (!nodesByBatch.TryGetValue(batchId, out Dictionary<ScannerResourceNodeKey, ScannerResourceNode>? batchNodes))
                {
                    continue;
                }

                foreach (ScannerResourceNode node in batchNodes.Values)
                {
                    NitroxVector3 delta = node.Position - center;
                    if (delta.X * delta.X + delta.Y * delta.Y + delta.Z * delta.Z <= radiusSquared)
                    {
                        result.Add(node);
                    }
                }
            }
        }

        return result;
    }

    private void AddOrReplace(WorldEntity entity)
    {
        List<ScannerResourceNode> nodes = CreateNodes(entity);
        lock (indexLock)
        {
            bool changed = RemoveEntityUnsafe(entity.Id);
            if (nodes.Count > 0)
            {
                List<(NitroxInt3 BatchId, ScannerResourceNodeKey Key)> entityKeys = new(nodes.Count);
                foreach (ScannerResourceNode node in nodes)
                {
                    if (!nodesByBatch.TryGetValue(node.BatchId, out Dictionary<ScannerResourceNodeKey, ScannerResourceNode>? batchNodes))
                    {
                        batchNodes = nodesByBatch[node.BatchId] = [];
                    }
                    batchNodes[node.Key] = node;
                    entityKeys.Add((node.BatchId, node.Key));
                }
                keysByEntity[entity.Id] = entityKeys;
                changed = true;
            }

            if (changed)
            {
                Interlocked.Increment(ref revision);
            }
        }
    }

    private List<ScannerResourceNode> CreateNodes(WorldEntity entity)
    {
        if (string.IsNullOrEmpty(entity.ClassId) || !resourceCatalog.TryGetDescriptors(entity.ClassId, out IReadOnlyList<ScannerResourceDescriptor>? descriptors))
        {
            return [];
        }

        List<ScannerResourceNode> nodes = new(descriptors.Count);
        foreach (ScannerResourceDescriptor descriptor in descriptors)
        {
            NitroxVector3 position = entity.Transform.LocalToWorldMatrix.Transform(descriptor.RelativePosition);
            NitroxInt3 batchId = new AbsoluteEntityCell(position, SubnauticaMap.ItemLevelOfDetail).BatchId;
            ScannerResourceNodeKey key = new(entity.Id, descriptor.TrackerIndex);
            nodes.Add(new ScannerResourceNode(key, descriptor.TechType, position, batchId));
        }
        return nodes;
    }

    private bool RemoveEntityUnsafe(NitroxId entityId)
    {
        if (!keysByEntity.Remove(entityId, out List<(NitroxInt3 BatchId, ScannerResourceNodeKey Key)>? keys))
        {
            return false;
        }

        foreach ((NitroxInt3 batchId, ScannerResourceNodeKey key) in keys)
        {
            if (!nodesByBatch.TryGetValue(batchId, out Dictionary<ScannerResourceNodeKey, ScannerResourceNode>? batchNodes))
            {
                continue;
            }
            batchNodes.Remove(key);
            if (batchNodes.Count == 0)
            {
                nodesByBatch.Remove(batchId);
            }
        }
        return true;
    }
}
