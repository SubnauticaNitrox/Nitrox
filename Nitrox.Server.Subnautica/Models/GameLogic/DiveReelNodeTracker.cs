using System.Collections.Generic;
using System.Threading;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

/// <summary>
/// Tracks each connected player's currently-placed DiveReel (Pathfinder Tool) node
/// positions in memory, so a newly-joining player can be sent everyone else's existing
/// trail in one shot. Capped at DiveReel's own maxNodes (20, see DiveReel.cs) per player --
/// intentionally not persisted to the world save; DiveReel.nodePositions on each player's
/// own tool instance already persists across sessions for that player individually, this
/// tracker only exists to answer "what does everyone ELSE currently have placed" for
/// newly-connecting clients while the server is running.
///
/// All public methods lock: LiteNetLibServer runs with UnsyncedEvents = true (see
/// LiteNetLibServer.cs), so NetworkReceiveEvent -- and therefore DiveReelNodePlacedProcessor /
/// DiveReelNodesResetProcessor -- fire directly on internal receive threads, not serialized
/// through one poll loop. Concurrent AddNode/ResetNodes racing a GetAllExcept/GetForPlayer
/// snapshot (e.g. during JoiningManager.SendInitialSync) is a real scenario, not theoretical.
/// </summary>
public class DiveReelNodeTracker
{
    private const int MaxNodesPerPlayer = 20;

    private readonly Lock nodesLock = new();
    private readonly Dictionary<ushort, List<NitroxVector3>> nodesByPlayer = new();

    public void AddNode(ushort playerId, NitroxVector3 position)
    {
        lock (nodesLock)
        {
            if (!nodesByPlayer.TryGetValue(playerId, out List<NitroxVector3> nodes))
            {
                nodes = new List<NitroxVector3>();
                nodesByPlayer[playerId] = nodes;
            }
            if (nodes.Count >= MaxNodesPerPlayer)
            {
                return;
            }
            nodes.Add(position);
            Log.Info($"AddNode player={playerId} pos={position} -> now {nodes.Count} node(s) tracked for this player");
        }
    }

    public void ResetNodes(ushort playerId)
    {
        lock (nodesLock)
        {
            nodesByPlayer.Remove(playerId);
            Log.Info($"ResetNodes player={playerId}");
        }
    }

    public Dictionary<ushort, List<NitroxVector3>> GetAllExcept(ushort excludedPlayerId)
    {
        lock (nodesLock)
        {
            Dictionary<ushort, List<NitroxVector3>> result = new();
            foreach (KeyValuePair<ushort, List<NitroxVector3>> entry in nodesByPlayer)
            {
                if (entry.Key != excludedPlayerId)
                {
                    result[entry.Key] = new List<NitroxVector3>(entry.Value);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Single-player snapshot of just <paramref name="playerId"/>'s currently-tracked nodes, in the
    /// same Dictionary shape DiveReelNodesInitialSync already uses (0 or 1 entries). Used to
    /// re-broadcast a rejoining player's already-tracked trail to everyone already connected --
    /// their earlier disconnect cleared this player's markers client-side elsewhere
    /// (DiveReelNodeMarkers via PlayerManager.OnRemove), and PlayerJoinedMultiplayerSession alone
    /// carries no DiveReel data, so without this those clients never get the trail back until the
    /// rejoining player places a new node or resets.
    /// </summary>
    public Dictionary<ushort, List<NitroxVector3>> GetForPlayer(ushort playerId)
    {
        lock (nodesLock)
        {
            Dictionary<ushort, List<NitroxVector3>> result = new();
            if (nodesByPlayer.TryGetValue(playerId, out List<NitroxVector3> nodes))
            {
                result[playerId] = new List<NitroxVector3>(nodes);
            }
            return result;
        }
    }
}
