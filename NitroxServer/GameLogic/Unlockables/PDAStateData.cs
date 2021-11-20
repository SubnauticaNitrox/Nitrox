using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Unlockables
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class PDAStateData
    {
        [JsonProperty, ProtoMember(1)]
        public ThreadSafeList<NitroxTechType> UnlockedTechTypes { get; } = new ThreadSafeList<NitroxTechType>();

        [JsonProperty, ProtoMember(2)]
        public ThreadSafeList<NitroxTechType> KnownTechTypes { get; } = new ThreadSafeList<NitroxTechType>();

        [JsonProperty, ProtoMember(3)]
        public ThreadSafeList<string> EncyclopediaEntries { get; } = new ThreadSafeList<string>();

        [JsonProperty, ProtoMember(4)]
        public ThreadSafeDictionary<NitroxTechType, PDAEntry> PartiallyUnlockedByTechType { get; set; } = new ThreadSafeDictionary<NitroxTechType, PDAEntry>();

        [JsonProperty, ProtoMember(5)]
        public ThreadSafeList<PDALogEntry> PdaLog { get; } = new ThreadSafeList<PDALogEntry>();

        [JsonProperty, ProtoMember(6)]
        public ThreadSafeDictionary<NitroxTechType, PDAProgressEntry> CachedProgress { get; } = new ThreadSafeDictionary<NitroxTechType, PDAProgressEntry>();

        public void UnlockedTechType(NitroxTechType techType)
        {
            PartiallyUnlockedByTechType.Remove(techType);
            CachedProgress.Remove(techType);
            UnlockedTechTypes.Add(techType);
        }

        public void AddKnownTechType(NitroxTechType techType)
        {
            KnownTechTypes.Add(techType);
        }

        public void AddEncyclopediaEntry(string entry)
        {
            EncyclopediaEntries.Add(entry);
        }

        public void AddPDALogEntry(PDALogEntry entry)
        {
            if (!PdaLog.Any(logEntry => logEntry.Key == entry.Key))
            {
                PdaLog.Add(entry);
            }
            else
            {
                Log.Debug($"There was an attempt of adding a duplicated entry in the PDALog: [{entry.Key}]");
            }
        }

        public void EntryProgressChanged(NitroxTechType techType, float progress, int unlocked, NitroxId nitroxId)
        {
            if (!PartiallyUnlockedByTechType.TryGetValue(techType, out PDAEntry pdaEntry))
            {
                PartiallyUnlockedByTechType[techType] = pdaEntry = new PDAEntry(techType, progress, unlocked);
            }

            // Update progress for specific entity if NitroxID is provided.
            if (nitroxId != null)
            {
                if (!CachedProgress.TryGetValue(techType, out PDAProgressEntry pdaProgressEntry))
                {
                    CachedProgress.Add(techType, pdaProgressEntry = new PDAProgressEntry(techType, new Dictionary<NitroxId, float>()));
                }
                // Prevents decreasing progress
                if (!pdaProgressEntry.Entries.ContainsKey(nitroxId) || (unlocked == pdaEntry.Unlocked && pdaProgressEntry.Entries.TryGetValue(nitroxId, out float oldProgress) && oldProgress < progress))
                {
                    pdaProgressEntry.Entries[nitroxId] = progress;
                    pdaEntry.Progress = progress;
                }
            }

            // This needs to occur after the progress update because
            // progress update needs to know what was the old unlocked state
            pdaEntry.Unlocked = unlocked;
        }

        public InitialPDAData GetInitialPDAData()
        {
            return new InitialPDAData(new List<NitroxTechType>(UnlockedTechTypes),
                new List<NitroxTechType>(KnownTechTypes),
                new List<string>(EncyclopediaEntries),
                new List<PDAEntry>(PartiallyUnlockedByTechType.Values),
                new List<PDALogEntry>(PdaLog),
                new List<PDAProgressEntry>(CachedProgress.Values));
        }
    }
}
