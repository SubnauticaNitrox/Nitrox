using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PDAScanFinishedProcessor : IClientPacketProcessor<PDAScanFinished>
{
    public Task Process(ClientProcessorContext context, PDAScanFinished packet)
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
        int previousUnlocked = PDAScanner.GetPartialEntryByKey(packetTechType, out PDAScanner.Entry entry) ? entry.unlocked : 0;
        PDAScanner.Entry updatedEntry = PDAScanner.Add(packetTechType, packet.UnlockedAmount);
        if (updatedEntry != null && updatedEntry.unlocked > previousUnlocked && Multiplayer.Main && Multiplayer.Main.InitialSyncCompleted)
        {
            int totalFragments = PDAScanner.GetEntryData(packetTechType)?.totalFragments ?? 1;
            if (totalFragments > 1)
            {
                float percentage = Mathf.RoundToInt((float)updatedEntry.unlocked / totalFragments * 100f);
                ErrorMessage.AddError(Language.main.GetFormat("ScannerInstanceScanned", Language.main.Get(packetTechType.AsString()), percentage, updatedEntry.unlocked, totalFragments));
            }
        }
        return Task.CompletedTask;
    }
}
