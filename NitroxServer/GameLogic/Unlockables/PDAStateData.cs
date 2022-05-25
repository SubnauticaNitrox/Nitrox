using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Unlockables
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class PDAStateData
    {
        /// <summary>
        /// Gets or sets the scan tool unlocked/partial unlock states.
        /// </summary>
        [JsonProperty, ProtoMember(1)]
        public ThreadSafeList<NitroxTechType> UnlockedTechTypes { get; } = new ThreadSafeList<NitroxTechType>();

        [JsonProperty, ProtoMember(4)]
        public ThreadSafeDictionary<NitroxTechType, PDAEntry> PartiallyUnlockedByTechType { get; set; } = new ThreadSafeDictionary<NitroxTechType, PDAEntry>();

        /// <summary>
        /// Gets or sets the KnownTech construct which powers the popup shown to the user when a new TechType is discovered ("New Creature Discovered!")
        /// The KnownTech construct uses both <see cref='NitroxModel.Packets.KnownTechEntryAdd.EntryCategory.KNOWN'>KnownTech.knownTech</see> and <see cref='NitroxModel.Packets.KnownTechEntryAdd.EntryCategory.ANALYZED'>KnownTech.analyzedTech</see>
        /// </summary>
        [JsonProperty, ProtoMember(2)]
        public ThreadSafeList<NitroxTechType> KnownTechTypes { get; } = new ThreadSafeList<NitroxTechType>();

        [JsonProperty, ProtoMember(7)]
        public ThreadSafeList<NitroxTechType> AnalyzedTechTypes { get; } = new ThreadSafeList<NitroxTechType>();

        /// <summary>
        /// Gets or sets the entries that show up the the PDA's Encyclopedia
        /// </summary>
        [JsonProperty, ProtoMember(3)]
        public ThreadSafeList<string> EncyclopediaEntries { get; } = new ThreadSafeList<string>();

        /// <summary>
        /// Gets or sets the log of story events present in the PDA
        /// </summary>
        [JsonProperty, ProtoMember(5)]
        public ThreadSafeList<PDALogEntry> PdaLog { get; } = new ThreadSafeList<PDALogEntry>();

        [JsonProperty, ProtoMember(6)]
        public ThreadSafeDictionary<NitroxTechType, PDAProgressEntry> CachedProgress { get; } = new ThreadSafeDictionary<NitroxTechType, PDAProgressEntry>();

        public void UnlockedTechType(NitroxTechType techType)
        {
            PartiallyUnlockedByTechType.Remove(techType);
            CachedProgress.Remove(techType);
            if (!UnlockedTechTypes.Contains(techType))
            {
                UnlockedTechTypes.Add(techType);
            }
            else
            {
                Log.Debug($"There was an attempt of adding a duplicated entry in the UnlockedTechTypes: [{techType.Name}]");
            }
        }

        public void AddKnownTechType(NitroxTechType techType)
        {
            if (!KnownTechTypes.Contains(techType))
            {
                KnownTechTypes.Add(techType);
            }
            else
            {
                Log.Debug($"There was an attempt of adding a duplicated entry in the KnownTechTypes: [{techType.Name}]");
            }
        }

        public void AddAnalyzedTechType(NitroxTechType techType)
        {
            if (!AnalyzedTechTypes.Contains(techType))
            {
                AnalyzedTechTypes.Add(techType);
            }
            else
            {
                Log.Debug($"There was an attempt of adding a duplicated entry in the AnalyzedTechTypes: [{techType.Name}]");
            }
        }

        public void AddEncyclopediaEntry(string entry)
        {
            if (!EncyclopediaEntries.Contains(entry))
            {
                EncyclopediaEntries.Add(entry);
            }
            else
            {
                Log.Debug($"There was an attempt of adding a duplicated entry in the EncyclopediaEntries: [{entry}]");
            }
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
                new List<NitroxTechType>(AnalyzedTechTypes),
                new List<string>(EncyclopediaEntries),
                new List<PDAEntry>(PartiallyUnlockedByTechType.Values),
                new List<PDALogEntry>(PdaLog),
                new List<PDAProgressEntry>(CachedProgress.Values));
        }
    }
}
