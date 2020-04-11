using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Helper;
using TechTypeModel = NitroxModel.DataStructures.TechType;
using NitroxModel_Subnautica.Helper;
using System.Collections;

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
            AddEncyclopediaEntries(packet.PDAData.EncyclopediaEntries);
            waitScreenItem.SetProgress(0.2f);
            yield return null;

            AddPdaEntryComplete(packet.PDAData.UnlockedTechTypes);
            waitScreenItem.SetProgress(0.4f);
            yield return null;

            AddPdaEntryPartial(packet.PDAData.PartiallyUnlockedTechTypes);
            waitScreenItem.SetProgress(0.6f);
            yield return null;

            AddKnownTech(packet.PDAData.KnownTechTypes);
            waitScreenItem.SetProgress(0.8f);
            yield return null;

            AddPdaLogs(packet.PDAData.PDALogEntries);
            waitScreenItem.SetProgress(1f);
            yield return null;
        }
        
        private void AddEncyclopediaEntries(List<string> newEntries)
        {
            Log.Info("Received initial sync packet with " + newEntries.Count + " encyclopedia entries");

            using (packetSender.Suppress<PDAEncyclopediaEntryAdd>())
            {
                foreach (string entry in newEntries)
                {
                    PDAEncyclopedia.Add(entry, false);
                }
            }
        }

        private void AddPdaEntryComplete(List<TechTypeModel> newEntries)
        {
            HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

            foreach (TechTypeModel item in newEntries)
            {
                complete.Add(item.Enum());
            }

            Log.Info("PDAEntryComplete save (added/new total): {pdaCompleteAdded}/{pdaCompleteTotal}", newEntries.Count, complete.Count);

        }

        private void AddPdaEntryPartial(List<PDAEntry> newEntries)
        {
            List<PDAScanner.Entry> partial = (List<PDAScanner.Entry>)(typeof(PDAScanner).GetField("partial", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

            foreach (PDAEntry entry in newEntries)
            {
                partial.Add(new PDAScanner.Entry { progress = entry.Progress, techType = entry.TechType.Enum(), unlocked = entry.Unlocked });
            }

            Log.Debug("PDAEntryPartial save (added/new total): {pdaPartialAdded}/{pdaPartialTotal}", newEntries.Count, partial.Count);
        }
        
        private void AddKnownTech(ICollection<TechTypeModel> newKnownTechTypes)
        {
            Log.Info("Received initial sync packet with {techTypeAdded} known tech types", newKnownTechTypes.Count);

            using (packetSender.Suppress<KnownTechEntryAdd>())
            {
                foreach (TechTypeModel techType in newKnownTechTypes)
                {
                    HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                    KnownTech.Add(techType.Enum(), false);
                }
            }
        }

        private void AddPdaLogs(ICollection<PDALogEntry> newPdaLogEntries)
        {
            Log.Info("Received initial sync packet with {pdaEntriesAdded} pda log entries", newPdaLogEntries.Count);

            using (packetSender.Suppress<PDALogEntryAdd>())
            {
                Dictionary<string, PDALog.Entry> entries = (Dictionary<string, PDALog.Entry>)(typeof(PDALog).GetField("entries", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

                foreach (PDALogEntry logEntry in newPdaLogEntries)
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
