using System.Collections.Generic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Settings;
using NitroxModel.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class BuildingDesyncWarningProcessor : IClientPacketProcessor<BuildingDesyncWarning>
{
    public Task Process(IPacketProcessContext context, BuildingDesyncWarning packet)
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
