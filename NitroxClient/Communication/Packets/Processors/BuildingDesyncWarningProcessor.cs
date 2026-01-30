using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Settings;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class BuildingDesyncWarningProcessor : IClientPacketProcessor<BuildingDesyncWarning>
{
    public Task Process(ClientProcessorContext context, BuildingDesyncWarning packet)
    {
        if (!BuildingHandler.Main)
        {
            return Task.CompletedTask;
        }

        foreach (KeyValuePair<NitroxId, int> operation in packet.Operations)
        {
            OperationTracker tracker = BuildingHandler.Main.EnsureTracker(operation.Key);
            tracker.LastOperationId = operation.Value;
            tracker.FailedOperations++;
        }

        if (NitroxPrefs.SafeBuildingLog.Value)
        {
            Log.InGame(Language.main.Get("Nitrox_BuildingDesyncDetected"));
        }
        return Task.CompletedTask;
    }
}
