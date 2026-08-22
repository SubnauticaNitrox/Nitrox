using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class DiveReelNodesInitialSyncProcessor : ClientPacketProcessor<DiveReelNodesInitialSync>
{
    private readonly DiveReelNodeMarkers nodeMarkers;

    public DiveReelNodesInitialSyncProcessor(DiveReelNodeMarkers nodeMarkers)
    {
        this.nodeMarkers = nodeMarkers;
    }

    public override void Process(DiveReelNodesInitialSync packet)
    {
        Log.Info($"Received InitialSync for {packet.PlayerNodes.Count} player(s): ({string.Join(", ", packet.PlayerNodes.Select(kvp => $"{kvp.Key}:{kvp.Value.Count}"))})");
        foreach (KeyValuePair<ushort, List<NitroxVector3>> entry in packet.PlayerNodes)
        {
            foreach (NitroxVector3 position in entry.Value)
            {
                nodeMarkers.SpawnMarker(entry.Key, position);
            }
        }
    }
}
