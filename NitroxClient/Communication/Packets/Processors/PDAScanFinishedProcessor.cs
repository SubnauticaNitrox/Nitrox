using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors;

public class PDAScanFinishedProcessor : ClientPacketProcessor<PDAScanFinished>
{
    public override void Process(PDAScanFinished packet)
    {
        if (packet.Id != null)
        {
            NitroxStoryManager.ScanCompleted(packet.Id, packet.Destroy);
        }
        if (packet.FullyResearched)
        {
            PDAScanner.partial.RemoveAll(entry => entry.techType.Equals(packet.TechType.ToUnity()));
            PDAScanner.complete.Add(packet.TechType.ToUnity());
            return;
        }
        if (PDAScanner.GetPartialEntryByKey(packet.TechType.ToUnity(), out PDAScanner.Entry entry))
        {
            entry.unlocked = packet.UnlockedAmount;
        }
        else
        {
            PDAScanner.Add(packet.TechType.ToUnity(), packet.UnlockedAmount);
        }
    }
}
