using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
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
            waitScreenItem.SetProgress(0.17f);
            yield return null;

            SetPDAEntryComplete(packet.PDAData.UnlockedTechTypes);
            waitScreenItem.SetProgress(0.33f);
            yield return null;

            SetPDAEntryPartial(packet.PDAData.PartiallyUnlockedTechTypes);
            waitScreenItem.SetProgress(0.5f);
            yield return null;

            SetKnownTech(packet.PDAData.KnownTechTypes, packet.PDAData.AnalyzedTechTypes);
            waitScreenItem.SetProgress(0.67f);
            yield return null;

            SetPDALog(packet.PDAData.PDALogEntries);
            waitScreenItem.SetProgress(0.83f);
            yield return null;

            SetCachedProgress(packet.PDAData.CachedProgress);
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
            HashSet<TechType> complete = PDAScanner.complete;

            foreach (NitroxTechType item in pdaEntryComplete)
            {
                complete.Add(item.ToUnity());
            }
            
            Log.Info($"PDAEntryComplete: New added: {pdaEntryComplete.Count}, Total: {complete.Count}");

        }

        private void SetPDAEntryPartial(List<PDAEntry> entries)
        {
            List<PDAScanner.Entry> partial = PDAScanner.partial;

            foreach (PDAEntry entry in entries)
            {
                // If, for some reason this happens, at least the client will be able to ignore it, in the other case, he wouldn't be able to connect
                if (entry.Progress == 0f)
                {
                    Log.Warn("A partial entry progress was set to 0 and was removed");
                    continue;
                }
                partial.Add(new PDAScanner.Entry { progress = entry.Progress, techType = entry.TechType.ToUnity(), unlocked = entry.Unlocked });
            }

            Log.Debug($"PDAEntryPartial: New added: {entries.Count}, Total: {partial.Count}");
        }

        private void SetKnownTech(List<NitroxTechType> knownTech, List<NitroxTechType> analyzedTech)
        {
            Log.Info($"Received initial sync packet with {knownTech.Count} KnownTech.knownTech types and {analyzedTech.Count} KnownTech.analyzedTech types.");

            using (packetSender.Suppress<KnownTechEntryAdd>())
            {
                foreach (NitroxTechType techType in knownTech)
                {
                    KnownTech.Add(techType.ToUnity(), false);
                }

                foreach (NitroxTechType techType in analyzedTech)
                {
                    KnownTech.Analyze(techType.ToUnity(), false);
                }
            }
        }

        private void SetPDALog(List<PDALogEntry> logEntries)
        {
            Log.Info($"Received initial sync packet with {logEntries.Count} pda log entries");

            using (packetSender.Suppress<PDALogEntryAdd>())
            {
                Dictionary<string, PDALog.Entry> entries = PDALog.entries;

                foreach (PDALogEntry logEntry in logEntries)
                {
                    if (logEntry.Key != null && !entries.ContainsKey(logEntry.Key))
                    {
                        if (PDALog.GetEntryData(logEntry.Key, out PDALog.EntryData entryData))
                        {
                            PDALog.Entry entry = new PDALog.Entry();
                            entry.data = entryData;
                            entry.timestamp = logEntry.Timestamp;
                            entries.Add(entryData.key, entry);

                            if (entryData.key == "Story_AuroraWarning4")
                            {
                                CrashedShipExploder.main.SwapModels(true);
                            }
                        }
                    }
                }
            }
        }

        private void SetCachedProgress(List<PDAProgressEntry> pdaCachedEntries)
        {
            Log.Info($"Received initial sync packet with {pdaCachedEntries.Count} cached progress entries");
            PDAManagerEntry.CachedEntries = pdaCachedEntries.ToDictionary(entry => entry.TechType);
        }
    }
}
