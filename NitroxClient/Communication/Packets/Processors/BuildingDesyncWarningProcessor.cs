using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Settings;
using NitroxClient.Services;
using NitroxClient.Services.Multiplayer;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class BuildingDesyncWarningProcessor(BuildingService buildingService) : IClientPacketProcessor<BuildingDesyncWarning>
{
    private readonly BuildingService buildingService = buildingService;

    public Task Process(ClientProcessorContext context, BuildingDesyncWarning packet)
    {
        foreach (KeyValuePair<NitroxId, int> operation in packet.Operations)
        {
            OperationTracker tracker = buildingService.EnsureTracker(operation.Key);
            tracker.LastOperationId = operation.Value;
            tracker.LocalOperations = 0;  // discard locally-queued ops, server's value is now authoritative
            tracker.FailedOperations++;
        }

        if (NitroxPrefs.SafeBuildingLog.Value)
        {
            Log.InGame(Language.main.Get("Nitrox_BuildingDesyncDetected"));
        }
        return Task.CompletedTask;
    }
}
