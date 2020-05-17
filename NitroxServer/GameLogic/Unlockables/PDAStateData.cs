using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Unlockables
{
    [ProtoContract]
    public class PDAStateData
    {
        [ProtoMember(1)]
        public ThreadSafeCollection<TechType> UnlockedTechTypes { get; } = new ThreadSafeCollection<TechType>();

        [ProtoMember(2)]
        public ThreadSafeCollection<TechType> KnownTechTypes { get; } = new ThreadSafeCollection<TechType>();

        [ProtoMember(3)]
        public ThreadSafeCollection<string> EncyclopediaEntries { get; } = new ThreadSafeCollection<string>();

        [ProtoMember(4)]
        public ThreadSafeDictionary<TechType, PDAEntry> PartiallyUnlockedByTechType { get; } = new ThreadSafeDictionary<TechType, PDAEntry>();

        [ProtoMember(5)]
        public ThreadSafeCollection<PDALogEntry> PdaLog { get; } = new ThreadSafeCollection<PDALogEntry>();

        public void UnlockedTechType(TechType techType)
        {
            PartiallyUnlockedByTechType.Remove(techType);
            UnlockedTechTypes.Add(techType);
        }

        public void AddKnownTechType(TechType techType)
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

        public void EntryProgressChanged(TechType techType, float progress, int unlocked)
        {
            PDAEntry pdaEntry;
            if (!PartiallyUnlockedByTechType.TryGetValue(techType, out pdaEntry))
            {
                PartiallyUnlockedByTechType[techType] = pdaEntry = new PDAEntry(techType, progress, unlocked);
            }

            pdaEntry.Progress = progress;
            pdaEntry.Unlocked = unlocked;
        }

        public InitialPDAData GetInitialPDAData()
        {
            return new InitialPDAData(new List<TechType>(UnlockedTechTypes),
                new List<TechType>(KnownTechTypes),
                new List<string>(EncyclopediaEntries),
                new List<PDAEntry>(PartiallyUnlockedByTechType.Values),
                new List<PDALogEntry>(PdaLog));
        }
    }
}
