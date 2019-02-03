﻿using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Helper;

namespace NitroxClient.GameLogic.InitialSync
{
    public class PdaInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;

        public PdaInitialSyncProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(InitialPlayerSync packet)
        {
            SetEncyclopediaEntry(packet.PDAData.EncyclopediaEntries);
            SetPDAEntryComplete(packet.PDAData.UnlockedTechTypes);
            SetPDAEntryPartial(packet.PDAData.PartiallyUnlockedTechTypes);
            SetKnownTech(packet.PDAData.KnownTechTypes);
            SetPDALog(packet.PDAData.PDALogEntries);
        }
        
        private void SetEncyclopediaEntry(List<EncyclopediaEntry> entries)
        {
            Log.Info("Received initial sync packet with " + entries.Count + " encyclopedia entries");

            using (packetSender.Suppress<PDAEncyclopediaEntryAdd>())
            {
                foreach (EncyclopediaEntry entry in entries)
                {
                    if (!entry.IsTimeCapsule)
                    {
                        PDAEncyclopedia.Add(entry.Key, false);
                    }
                    else
                    {
                        PDAEncyclopedia.AddTimeCapsule(entry.Key, false);
                    }
                }
            }
        }

        private void SetPDAEntryComplete(List<TechType> pdaEntryComplete)
        {
            HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

            foreach (TechType item in pdaEntryComplete)
            {
                complete.Add(item);
            }

            Log.Info("PDAEntryComplete Save:" + pdaEntryComplete.Count + " Read Partial Client Final Count:" + complete.Count);

        }

        private void SetPDAEntryPartial(List<PDAEntry> entries)
        {
            List<PDAScanner.Entry> partial = (List<PDAScanner.Entry>)(typeof(PDAScanner).GetField("partial", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

            foreach (PDAEntry entry in entries)
            {
                partial.Add(new PDAScanner.Entry { progress = entry.Progress, techType = entry.TechType, unlocked = entry.Unlocked });
            }

            Log.Info("PDAEntryPartial Save :" + entries.Count + " Read Partial Client Final Count:" + partial.Count);
        }
        
        private void SetKnownTech(List<TechType> techTypes)
        {
            Log.Info("Received initial sync packet with " + techTypes.Count + " known tech types");

            using (packetSender.Suppress<KnownTechEntryAdd>())
            {
                foreach (TechType techType in techTypes)
                {
                    HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                    KnownTech.Add(techType, false);
                }
            }
        }

        private void SetPDALog(List<PDALogEntry> logEntries)
        {
            Log.Info("Received initial sync packet with " + logEntries.Count + " pda log entries");

            using (packetSender.Suppress<PDALogEntryAdd>())
            {
                Dictionary<string, PDALog.Entry> entries = (Dictionary<string, PDALog.Entry>)(typeof(PDALog).GetField("entries", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

                foreach (PDALogEntry logEntry in logEntries)
                {
                    if (!entries.ContainsKey(logEntry.Key))
                    {
                        PDALog.EntryData entryData;
                        PDALog.GetEntryData(logEntry.Key, out entryData);
                        PDALog.Entry entry = new PDALog.Entry();
                        entry.data = entryData;
                        entry.timestamp = logEntry.Timestamp;
                        entries.Add(entryData.key, entry);

                        if (entryData.key == "Story_AuroraWarning4")
                        {
                            CrashedShipExploder.main.ReflectionCall("SwapModels", false, false, new object[] { true });
                        }
                    }
                }
            }
        }
    }
}
