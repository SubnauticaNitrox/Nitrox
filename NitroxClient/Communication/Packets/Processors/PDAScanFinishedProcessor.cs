using NitroxClient.GameLogic;
using NitroxModel;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PDAScanFinishedProcessor : IClientPacketProcessor<PdaScanFinished>
{
    public Task Process(IPacketProcessContext context, PdaScanFinished packet)
    {
        if (packet.Id != null)
        {
            StoryManager.ScanCompleted(packet.Id, packet.Destroy);
        }
        if (packet.WasAlreadyResearched)
        {
            return Task.CompletedTask;
        }
        TechType packetTechType = packet.TechType.ToUnity();
        if (packet.FullyResearched)
        {
            PDAScanner.partial.RemoveAllFast(packetTechType, static (item, techType) => item.techType == techType);
            PDAScanner.complete.Add(packetTechType);
            return Task.CompletedTask;
        }
        if (PDAScanner.GetPartialEntryByKey(packetTechType, out PDAScanner.Entry entry))
        {
            entry.unlocked = packet.UnlockedAmount;
        }
        else
        {
            PDAScanner.Add(packetTechType, packet.UnlockedAmount);
        }

        return Task.CompletedTask;
    }
}
