using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Unlockables
{
    [ProtoContract]
    public class PDAStateData
    {
        [ProtoMember(1)]
        public List<TechType> SerializedUnlockedTechTypes
        {
            get
            {
                lock (unlockedTechTypes)
                {
                    return unlockedTechTypes;
                }
            }
            set { unlockedTechTypes = value; }
        }

        [ProtoMember(2)]
        public List<TechType> SerializedKnownTechTypes
        {
            get
            {
                lock (knownTechTypes)
                {
                    return knownTechTypes;
                }
            }
            set { knownTechTypes = value; }
        }

        [ProtoMember(3)]
        public List<string> SerializedEncyclopediaEntries
        {
            get
            {
                lock (encyclopediaEntries)
                {
                    return encyclopediaEntries;
                }
            }
            set { encyclopediaEntries = value; }
        }

        [ProtoMember(4)]
        public List<PDAEntry> SerializedPartiallyUnlockedByTechType
        {
            get
            {
                lock (partiallyUnlockedByTechType)
                {
                    return new List<PDAEntry>(partiallyUnlockedByTechType.Values);
                }
            }
            set
            {
                partiallyUnlockedByTechType = new Dictionary<TechType, PDAEntry>();

                foreach (PDAEntry entry in value)
                {
                    partiallyUnlockedByTechType.Add(entry.TechType, entry);
                }
            }
        }

        [ProtoMember(5)]
        public List<PDALogEntry> SerializedPDALog
        {
            get
            {
                lock (pdaLogEntries)
                {
                    return pdaLogEntries;
                }
            }
            set { pdaLogEntries = value; }
        }

        [ProtoIgnore]
        private List<TechType> unlockedTechTypes = new List<TechType>();

        [ProtoIgnore]
        private List<TechType> knownTechTypes = new List<TechType>();

        [ProtoIgnore]
        private List<string> encyclopediaEntries = new List<string>();

        [ProtoIgnore]
        private Dictionary<TechType, PDAEntry> partiallyUnlockedByTechType = new Dictionary<TechType, PDAEntry>();

        [ProtoIgnore]
        private List<PDALogEntry> pdaLogEntries = new List<PDALogEntry>();

        public void UnlockedTechType(TechType techType)
        {
            lock(unlockedTechTypes)
            {
                lock (partiallyUnlockedByTechType)
                {
                    partiallyUnlockedByTechType.Remove(techType);
                    unlockedTechTypes.Add(techType);
                }
            }
        }

        public void AddKnownTechType(TechType techType)
        {
            lock (knownTechTypes)
            {
                knownTechTypes.Add(techType);
            }
        }

        public void AddEncyclopediaEntry(string entry)
        {
            lock (encyclopediaEntries)
            {
                encyclopediaEntries.Add(entry);
            }
        }

        public void AddPDALogEntry(PDALogEntry entry)
        {
            lock (pdaLogEntries)
            {
                pdaLogEntries.Add(entry);
            }
        }

        public void EntryProgressChanged(TechType techType, float progress, int unlocked)
        {
            lock (partiallyUnlockedByTechType)
            {
                PDAEntry pdaEntry = null;

                if(!partiallyUnlockedByTechType.TryGetValue(techType, out pdaEntry))
                {
                    partiallyUnlockedByTechType[techType] = pdaEntry = new PDAEntry(techType, progress, unlocked);
                }

                pdaEntry.Progress = progress;
                pdaEntry.Unlocked = unlocked;
            }
        }

        public InitialPdaData GetInitialPdaData()
        {
            lock (unlockedTechTypes)
            {
                lock (partiallyUnlockedByTechType)
                {
                    lock (knownTechTypes)
                    {
                        lock (encyclopediaEntries)
                        {
                            lock (pdaLogEntries)
                            {
                                return new InitialPdaData(new List<TechType>(unlockedTechTypes),
                                                          new List<TechType>(knownTechTypes),
                                                          new List<string>(encyclopediaEntries),
                                                          new List<PDAEntry>(partiallyUnlockedByTechType.Values),
                                                          new List<PDALogEntry>(pdaLogEntries));
                            }
                        }
                    }
                }
            }
        }
    }
}
