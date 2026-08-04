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

    public void EntityTracked(WorldEntity entity) => AddOrReplace(entity, false);

    public void EntityMoved(WorldEntity entity) => AddOrReplace(entity, true);

    /// <summary>
    /// Replaces the index contents with entities restored directly into the world services.
    /// This is intentionally separate from lifecycle notifications because save restoration
    /// bypasses normal entity tracking events.
    /// </summary>
    public void Hydrate(IEnumerable<WorldEntity> restoredEntities)
    {
        Dictionary<NitroxInt3, Dictionary<ScannerResourceNodeKey, ScannerResourceNode>> hydratedNodesByBatch = [];
        Dictionary<NitroxId, List<(NitroxInt3 BatchId, ScannerResourceNodeKey Key)>> hydratedKeysByEntity = [];

        foreach (WorldEntity entity in restoredEntities)
        {
            RemoveEntity(entity.Id, hydratedNodesByBatch, hydratedKeysByEntity);

            List<ScannerResourceNode> nodes = CreateNodes(entity);
            if (nodes.Count == 0)
            {
                continue;
            }

            List<(NitroxInt3 BatchId, ScannerResourceNodeKey Key)> entityKeys = new(nodes.Count);
            foreach (ScannerResourceNode node in nodes)
            {
                if (!hydratedNodesByBatch.TryGetValue(node.BatchId, out Dictionary<ScannerResourceNodeKey, ScannerResourceNode>? batchNodes))
                {
                    batchNodes = hydratedNodesByBatch[node.BatchId] = [];
                }
                batchNodes[node.Key] = node;
                entityKeys.Add((node.BatchId, node.Key));
            }
            hydratedKeysByEntity[entity.Id] = entityKeys;
        }

        lock (indexLock)
        {
            bool changed = !HasSameNodesUnsafe(hydratedNodesByBatch);

            nodesByBatch.Clear();
            foreach ((NitroxInt3 batchId, Dictionary<ScannerResourceNodeKey, ScannerResourceNode> nodes) in hydratedNodesByBatch)
            {
                nodesByBatch.Add(batchId, nodes);
            }

            keysByEntity.Clear();
            foreach ((NitroxId entityId, List<(NitroxInt3 BatchId, ScannerResourceNodeKey Key)> keys) in hydratedKeysByEntity)
            {
                keysByEntity.Add(entityId, keys);
            }

            if (changed)
            {
                Interlocked.Increment(ref revision);
            }
        }
    }

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

    private void AddOrReplace(WorldEntity entity, bool requireTrackedEntity)
    {
        lock (indexLock)
        {
            // Movement notifications are published after the world mutation lock is released. A concurrent pickup or
            // destruction can therefore untrack the entity before an older movement callback reaches this observer.
            // Never let that late callback recreate an entity which is no longer part of the world index.
            if (requireTrackedEntity && !keysByEntity.ContainsKey(entity.Id))
            {
                return;
            }

            // Read the transform under the index lock so overlapping movement callbacks always publish the entity's
            // latest committed transform instead of a position captured before a newer callback acquired this lock.
            List<ScannerResourceNode> nodes = CreateNodes(entity);
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
        return RemoveEntity(entityId, nodesByBatch, keysByEntity);
    }

    private bool HasSameNodesUnsafe(Dictionary<NitroxInt3, Dictionary<ScannerResourceNodeKey, ScannerResourceNode>> otherNodesByBatch)
    {
        if (nodesByBatch.Count != otherNodesByBatch.Count)
        {
            return false;
        }

        foreach ((NitroxInt3 batchId, Dictionary<ScannerResourceNodeKey, ScannerResourceNode> otherBatchNodes) in otherNodesByBatch)
        {
            if (!nodesByBatch.TryGetValue(batchId, out Dictionary<ScannerResourceNodeKey, ScannerResourceNode>? batchNodes) ||
                batchNodes.Count != otherBatchNodes.Count)
            {
                return false;
            }

            foreach ((ScannerResourceNodeKey key, ScannerResourceNode otherNode) in otherBatchNodes)
            {
                if (!batchNodes.TryGetValue(key, out ScannerResourceNode node) || !node.Equals(otherNode))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool RemoveEntity(
        NitroxId entityId,
        Dictionary<NitroxInt3, Dictionary<ScannerResourceNodeKey, ScannerResourceNode>> targetNodesByBatch,
        Dictionary<NitroxId, List<(NitroxInt3 BatchId, ScannerResourceNodeKey Key)>> targetKeysByEntity)
    {
        if (!targetKeysByEntity.Remove(entityId, out List<(NitroxInt3 BatchId, ScannerResourceNodeKey Key)>? keys))
        {
            return false;
        }

        foreach ((NitroxInt3 batchId, ScannerResourceNodeKey key) in keys)
        {
            if (!targetNodesByBatch.TryGetValue(batchId, out Dictionary<ScannerResourceNodeKey, ScannerResourceNode>? batchNodes))
            {
                continue;
            }
            batchNodes.Remove(key);
            if (batchNodes.Count == 0)
            {
                targetNodesByBatch.Remove(batchId);
            }
        }
        return true;
    }
}
