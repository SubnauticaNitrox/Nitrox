using System;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAScannerEntryProcessorAdd : ClientPacketProcessor<PDAEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public PDAScannerEntryProcessorAdd(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEntryAdd packet)
        {
            using (packetSender.Suppress<PDAEntryAdd>())
            using (packetSender.Suppress<PDAEntryProgress>())
            {
                PDAScanner.EntryData entryData = PDAScanner.GetEntryData(packet.TechType);

                PDAScanner.Entry entry;
                if (!PDAScanner.GetPartialEntryByKey(packet.TechType, out entry))
                {
                    MethodInfo methodAdd = typeof(PDAScanner).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(TechType), typeof(int) }, null);
                    entry = (PDAScanner.Entry)methodAdd.Invoke(null, new object[] { packet.TechType, packet.Unlocked });

                }

                if (entry != null)
                {
                    entry.unlocked++;

                    if (entry.unlocked >= entryData.totalFragments)
                    {
                        List<PDAScanner.Entry> partial = (List<PDAScanner.Entry>)(typeof(PDAScanner).GetField("partial", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                        HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                        partial.Remove(entry);
                        complete.Add(entry.techType);
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

    public class PDAScannerEntryProcessorProgress : ClientPacketProcessor<PDAEntryProgress>
    {
        private readonly IPacketSender packetSender;

        public PDAScannerEntryProcessorProgress(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEntryProgress packet)
        {
            using (packetSender.Suppress<PDAEntryAdd>())
            using (packetSender.Suppress<PDAEntryProgress>())
            {
                PDAScanner.EntryData entryData = PDAScanner.GetEntryData(packet.TechType);

                PDAScanner.Entry entry;
                if (PDAScanner.GetPartialEntryByKey(packet.TechType, out entry))
                {
                    if (packet.Unlocked > entry.unlocked)
                    {
                        Log.Info("PDAEntryProgress Upldate Old:" + entry.unlocked + " New" + packet.Unlocked);
                        entry.unlocked = packet.Unlocked;
                    }
                }
                else
                {
                    Log.Info("PDAEntryProgress New TechType:" + packet.TechType + " Unlocked:" + packet.Unlocked);
                    MethodInfo methodAdd = typeof(PDAScanner).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(TechType), typeof(int) }, null);
                    entry = (PDAScanner.Entry)methodAdd.Invoke(null, new object[] { packet.TechType, packet.Unlocked });
                }
            }
        }
    }

    public class PDAScannerEntryProcessorRemove : ClientPacketProcessor<PDAEntryRemove>
    {
        private readonly IPacketSender packetSender;

        public PDAScannerEntryProcessorRemove(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEntryRemove packet)
        {
            using (packetSender.Suppress<PDAEntryRemove>())
            {
                PDAScanner.Entry entry;
                if (PDAScanner.GetPartialEntryByKey(packet.TechType, out entry))
                {
                    List<PDAScanner.Entry> partial = (List<PDAScanner.Entry>)(typeof(PDAScanner).GetField("partial", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                    HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                    partial.Remove(entry);
                    complete.Add(entry.techType);
                }
            }
        }
    }
}
