using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
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
            UnlockedTechTypes.Add(techType);
            if (CachedProgress.TryGetValue(techType, out PDAProgressEntry pdaProgressEntry))
            {
                CachedProgress.Remove(techType);
            }
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
            PdaLog.Add(entry);
        }

        public void EntryProgressChanged(NitroxTechType techType, float progress, int unlocked, string id)
        {
            if (!PartiallyUnlockedByTechType.TryGetValue(techType, out PDAEntry pdaEntry))
            {
                PartiallyUnlockedByTechType[techType] = pdaEntry = new PDAEntry(techType, progress, unlocked);
            }

            pdaEntry.Progress = progress;
            pdaEntry.Unlocked = unlocked;
            
            // Stuff concerning PDA CachedEntries
            PDAProgressEntry pdaProgressEntry;
            bool exists = CachedProgress.TryGetValue(techType, out pdaProgressEntry);
            if (id != null && id.Length > 0 && !exists)
            {
                CachedProgress.Add(techType, pdaProgressEntry = new PDAProgressEntry(techType, new Dictionary<NitroxId, float>()));
            }

            pdaProgressEntry.Entries[new NitroxId(id)] = progress;
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
