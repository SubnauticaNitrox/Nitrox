using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Bases.New;
using NitroxClient.GameLogic.Settings;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using System.Collections.Generic;

namespace NitroxClient.Communication.Packets.Processors;

public class BuildingDesyncWarningProcessor : ClientPacketProcessor<BuildingDesyncWarning>
{
    public override void Process(BuildingDesyncWarning packet)
    {        
        if (!BuildingTester.Main)
        {
            return;
        }
        foreach (KeyValuePair<NitroxId, int> operation in packet.Operations)
        {
            OperationTracker tracker = BuildingTester.Main.EnsureTracker(operation.Key);
            tracker.LastOperationId = operation.Value;
            tracker.FailedOperations++;
        }
        if (NitroxPrefs.SafeBuildingLog.Value)
        {
            // TODO: Localize
            ErrorMessage.AddMessage($"Server detected a desync with the local client's buildings (go to Nitrox settings to request a resync)");
        }
    }
}
