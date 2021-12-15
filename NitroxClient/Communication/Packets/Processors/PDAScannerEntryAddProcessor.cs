using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAScannerEntryAddProcessor : ClientPacketProcessor<PDAEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public PDAScannerEntryAddProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEntryAdd packet)
        {
            using (packetSender.Suppress<PDAEntryAdd>())
            using (packetSender.Suppress<PDAEntryProgress>())
            {
                TechType techType = packet.TechType.ToUnity();
                PDAScanner.EntryData entryData = PDAScanner.GetEntryData(techType);

                if (!PDAScanner.GetPartialEntryByKey(techType, out PDAScanner.Entry entry))
                {
                    entry = PDAScanner.Add(techType, packet.Unlocked);
                }

                if (entry != null)
                {
                    entry.unlocked++;

                    if (entry.unlocked >= entryData.totalFragments)
                    {
                        PDAScanner.partial.Remove(entry);
                        PDAScanner.complete.Add(entry.techType);
                    }
                    else
                    {
                        int totalFragments = entryData.totalFragments;
                        if (totalFragments > 1)
                        {
                            float num2 = (float)entry.unlocked / (float)totalFragments;
                            float arg = (float)Mathf.RoundToInt(num2 * 100f);
                            ErrorMessage.AddError(Language.main.GetFormat<string, float, int, int>("ScannerInstanceScanned", Language.main.Get(entry.techType.AsString(false)), arg, entry.unlocked, totalFragments));
                        }
                    }
                }
            }
        }
    }
}
