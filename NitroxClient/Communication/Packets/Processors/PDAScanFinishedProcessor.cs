using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors;

public class PDAScanFinishedProcessor : ClientPacketProcessor<PDAScanFinished>
{
    public override void Process(PDAScanFinished packet)
    {
        if (packet.Id != null)
        {
            StoryManager.ScanCompleted(packet.Id, packet.Destroy);
        }
        if (packet.WasAlreadyResearched)
        {
            return;
        }
        TechType packetTechType = packet.TechType.ToUnity();
        if (packet.FullyResearched)
        {
            PDAScanner.partial.RemoveAllFast(packetTechType, static (item, techType) => item.techType == techType);
            PDAScanner.complete.Add(packetTechType);
            return;
        }
        if (PDAScanner.GetPartialEntryByKey(packetTechType, out PDAScanner.Entry entry))
        {
            entry.unlocked = packet.UnlockedAmount;
        }
        else
        {
            PDAScanner.Add(packetTechType, packet.UnlockedAmount);
        }
    }
}
