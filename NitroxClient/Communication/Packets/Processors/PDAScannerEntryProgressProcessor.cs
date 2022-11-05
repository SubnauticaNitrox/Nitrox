using System;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAScannerEntryProgressProcessor : ClientPacketProcessor<PDAEntryProgress>
    {
        private readonly IPacketSender packetSender;

        public PDAScannerEntryProgressProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEntryProgress packet)
        {
            TechType techType = packet.TechType.ToUnity();

            if (PDAScanner.GetPartialEntryByKey(techType, out PDAScanner.Entry entry))
            {
                if (packet.Unlocked == entry.unlocked)
                {
                    // Add the entry as a cached progress
                    if (!PDAManagerEntry.CachedEntries.TryGetValue(packet.TechType, out PDAProgressEntry pdaProgressEntry))
                    {
                        PDAManagerEntry.CachedEntries.Add(packet.TechType, pdaProgressEntry = new PDAProgressEntry(packet.TechType, new Dictionary<NitroxId, float>()));
                    }
                    pdaProgressEntry.Entries[packet.NitroxId] = packet.Progress;
                }
                else if (packet.Unlocked > entry.unlocked)
                {
                    Log.Info($"PDAEntryProgress Update For TechType:{techType} Old:{entry.unlocked} New:{packet.Unlocked}");
                    entry.unlocked = packet.Unlocked;
                }
            }
            else
            {
                Log.Info($"PDAEntryProgress New TechType:{techType} Unlocked:{packet.Unlocked}");
                PDAScanner.Add(techType, packet.Unlocked);
            }
        }
    }
}
