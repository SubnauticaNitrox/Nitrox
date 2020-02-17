using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;
using TechTypeModel = NitroxModel.DataStructures.TechType;

namespace NitroxServer.GameLogic.Unlockables
{
    [ProtoContract]
    public class PDAStateData
    {
        [ProtoMember(1)]
        public List<TechTypeModel> SerializedUnlockedTechTypes
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
        public List<TechTypeModel> SerializedKnownTechTypes
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
                    serializedPartiallyUnlockedByTechType = new List<PDAEntry>(partiallyUnlockedByTechType.Values);
                    return serializedPartiallyUnlockedByTechType;
                }
            }
            set
            {
                lock (partiallyUnlockedByTechType)
                {
                    partiallyUnlockedByTechType = new Dictionary<TechTypeModel, PDAEntry>();

                    foreach (PDAEntry entry in value)
                    {
                        partiallyUnlockedByTechType.Add(entry.TechType, entry);
                    }
                }

                serializedPartiallyUnlockedByTechType = value;
            }
        }

        private List<PDAEntry> serializedPartiallyUnlockedByTechType = new List<PDAEntry>();

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
        private List<TechTypeModel> unlockedTechTypes = new List<TechTypeModel>();

        [ProtoIgnore]
        private List<TechTypeModel> knownTechTypes = new List<TechTypeModel>();

        [ProtoIgnore]
        private List<string> encyclopediaEntries = new List<string>();

        [ProtoIgnore]
        private Dictionary<TechTypeModel, PDAEntry> partiallyUnlockedByTechType = new Dictionary<TechTypeModel, PDAEntry>();

        [ProtoIgnore]
        private List<PDALogEntry> pdaLogEntries = new List<PDALogEntry>();

        public void UnlockedTechType(TechTypeModel techType)
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

        public void AddKnownTechType(TechTypeModel techType)
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

        public void EntryProgressChanged(TechTypeModel techType, float progress, int unlocked)
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
                                return new InitialPdaData(new List<TechTypeModel>(unlockedTechTypes),
                                                          new List<TechTypeModel>(knownTechTypes),
                                                          new List<string>(encyclopediaEntries),
                                                          new List<PDAEntry>(partiallyUnlockedByTechType.Values),
                                                          new List<PDALogEntry>(pdaLogEntries));
                            }
                        }
                    }
                }
            }
        }

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            lock (partiallyUnlockedByTechType)
            {
                partiallyUnlockedByTechType = new Dictionary<TechTypeModel, PDAEntry>();
                foreach (PDAEntry entry in serializedPartiallyUnlockedByTechType)
                {
                    partiallyUnlockedByTechType.Add(entry.TechType, entry);
                }
            }
        }
    }
}
