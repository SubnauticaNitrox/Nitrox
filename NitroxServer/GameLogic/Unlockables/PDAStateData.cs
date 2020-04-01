using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using TechTypeModel = NitroxModel.DataStructures.TechType;

namespace NitroxServer.GameLogic.Unlockables
{
    [ProtoContract]
    public class PDAStateData
    {
        [ProtoMember(1)]
        public ThreadSafeCollection<TechTypeModel> UnlockedTechTypes { get; } = new ThreadSafeCollection<TechType>();

        [ProtoMember(2)]
        public ThreadSafeCollection<TechTypeModel> KnownTechTypes { get; } = new ThreadSafeCollection<TechType>();

        [ProtoMember(3)]
        public ThreadSafeCollection<string> EncyclopediaEntries { get; } = new ThreadSafeCollection<string>();

        [ProtoMember(4)]
        public ThreadSafeDictionary<TechTypeModel, PDAEntry> PartiallyUnlockedByTechType { get; } = new ThreadSafeDictionary<TechTypeModel, PDAEntry>();

        [ProtoMember(5)]
        public ThreadSafeCollection<PDALogEntry> PdaLog { get; } = new ThreadSafeCollection<PDALogEntry>();

        public void UnlockedTechType(TechTypeModel techType)
        {
            PartiallyUnlockedByTechType.Remove(techType);
            UnlockedTechTypes.Add(techType);
        }

        public void AddKnownTechType(TechTypeModel techType)
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

        public void EntryProgressChanged(TechTypeModel techType, float progress, int unlocked)
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
            return new InitialPDAData(new List<TechTypeModel>(UnlockedTechTypes),
                new List<TechTypeModel>(KnownTechTypes),
                new List<string>(EncyclopediaEntries),
                new List<PDAEntry>(PartiallyUnlockedByTechType.Values),
                new List<PDALogEntry>(PdaLog));
        }
    }
}
