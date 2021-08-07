using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.InitialSync
{
    public class PdaInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;

        public PdaInitialSyncProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            SetEncyclopediaEntry(packet.PDAData.EncyclopediaEntries);
            waitScreenItem.SetProgress(0.2f);
            yield return null;

            SetPDAEntryComplete(packet.PDAData.UnlockedTechTypes);
            waitScreenItem.SetProgress(0.4f);
            yield return null;

            SetPDAEntryPartial(packet.PDAData.PartiallyUnlockedTechTypes);
            waitScreenItem.SetProgress(0.6f);
            yield return null;

            SetKnownTech(packet.PDAData.KnownTechTypes);
            waitScreenItem.SetProgress(0.8f);
            yield return null;

            SetPDALog(packet.PDAData.PDALogEntries);
            waitScreenItem.SetProgress(1f);
            yield return null;
        }

        private void SetEncyclopediaEntry(List<string> entries)
        {
            Log.Info($"Received initial sync packet with {entries.Count} encyclopedia entries");

            using (packetSender.Suppress<PDAEncyclopediaEntryAdd>())
            {
                foreach (string entry in entries)
                {
                    PDAEncyclopedia.Add(entry, false);
                }
            }
        }

        private void SetPDAEntryComplete(List<NitroxTechType> pdaEntryComplete)
        {
            HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

            foreach (NitroxTechType item in pdaEntryComplete)
            {
                complete.Add(item.ToUnity());
            }

            Log.Info($"PDAEntryComplete: New added: {pdaEntryComplete.Count}, Total: {complete.Count}");

        }

        private void SetPDAEntryPartial(List<PDAEntry> entries)
        {
            List<PDAScanner.Entry> partial = (List<PDAScanner.Entry>)(typeof(PDAScanner).GetField("partial", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
            
            foreach (PDAEntry entry in entries)
            {
                PDAScanner.Entry newEntry = new PDAScanner.Entry { progress = entry.Progress, techType = entry.TechType.ToUnity(), unlocked = entry.Unlocked };
                // If, for some reason this happens, at least the client will be able to ignore it, in the other case, he wouldn't be able to connect
                if (newEntry.progress != 0f)
                {
                    partial.Add(newEntry);
                    Log.Debug($"New partial entry: {newEntry}");
                }
            }
            Log.Debug($"PDAEntryPartial: New added: {entries.Count}, Total: {partial.Count}");
        }

        private void SetKnownTech(List<NitroxTechType> techTypes)
        {
            Log.Info($"Received initial sync packet with {techTypes.Count} known tech types");

            using (packetSender.Suppress<KnownTechEntryAdd>())
            {
                foreach (NitroxTechType techType in techTypes)
                {
                    HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                    KnownTech.Add(techType.ToUnity(), false);
                }
            }
        }

        private void SetPDALog(List<PDALogEntry> logEntries)
        {
            Log.Info($"Received initial sync packet with {logEntries.Count} pda log entries");

            using (packetSender.Suppress<PDALogEntryAdd>())
            {
                Dictionary<string, PDALog.Entry> entries = (Dictionary<string, PDALog.Entry>)(typeof(PDALog).GetField("entries", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

                foreach (PDALogEntry logEntry in logEntries)
                {
                    if (!entries.ContainsKey(logEntry.Key))
                    {
                        PDALog.GetEntryData(logEntry.Key, out PDALog.EntryData entryData);
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
