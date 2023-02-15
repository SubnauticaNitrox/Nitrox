using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PDAScanProgressProcessor : ClientPacketProcessor<PDAScanProgress>
{
    public override void Process(PDAScanProgress packet)
    {
        // Fully-scanned entity
        if (packet.ProgressCompleted)
        {
            StoryManager.ScanCompleted(packet.Id, packet.Destroy);
            return;
        }

        // We only want to update the current progress if the new one is greater
        if (PDAScanner.cachedProgress.TryGetValue(packet.Id.ToString(), out float currentProgress))
        {
            if (currentProgress >= packet.Progress)
            {
                return;
            }
        }
        // We can store the progress associated to a NitroxId because in PDAScanner_ScanTarget_Initialize_Patch,
        // we set ScanTargets' id to be from their NitroxEntity if they have one
        PDAScanner.cachedProgress[packet.Id.ToString()] = currentProgress;
    }
}
