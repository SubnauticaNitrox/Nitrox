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
        public ThreadSafeCollection<NitroxTechType> UnlockedTechTypes { get; } = new ThreadSafeCollection<NitroxTechType>();

        [ProtoMember(2)]
        public ThreadSafeCollection<NitroxTechType> KnownTechTypes { get; } = new ThreadSafeCollection<NitroxTechType>();

        [ProtoMember(3)]
        public ThreadSafeCollection<string> EncyclopediaEntries { get; } = new ThreadSafeCollection<string>();

        [ProtoMember(4)]
        public ThreadSafeDictionary<NitroxTechType, PDAEntry> PartiallyUnlockedByTechType { get; } = new ThreadSafeDictionary<NitroxTechType, PDAEntry>();

        [ProtoMember(5)]
        public ThreadSafeCollection<PDALogEntry> PdaLog { get; } = new ThreadSafeCollection<PDALogEntry>();

        public void UnlockedTechType(NitroxTechType techType)
        {
            PartiallyUnlockedByTechType.Remove(techType);
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
            PdaLog.Add(entry);
        }

        public void EntryProgressChanged(NitroxTechType techType, float progress, int unlocked)
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
            return new InitialPDAData(new List<NitroxTechType>(UnlockedTechTypes),
                new List<NitroxTechType>(KnownTechTypes),
                new List<string>(EncyclopediaEntries),
                new List<PDAEntry>(PartiallyUnlockedByTechType.Values),
                new List<PDALogEntry>(PdaLog));
        }
    }
}
