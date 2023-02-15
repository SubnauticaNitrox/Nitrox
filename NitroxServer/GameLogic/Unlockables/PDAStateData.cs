using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Unlockables
{
    [DataContract]
    public class PDAStateData
    {
        /// <summary>
        /// Gets or sets the KnownTech construct which powers the popup shown to the user when a new TechType is discovered ("New Creature Discovered!")
        /// The KnownTech construct uses both <see cref='NitroxModel.Packets.KnownTechEntryAdd.EntryCategory.KNOWN'>KnownTech.knownTech</see> and <see cref='NitroxModel.Packets.KnownTechEntryAdd.EntryCategory.ANALYZED'>KnownTech.analyzedTech</see>
        /// </summary>
        [DataMember(Order = 1)]
        public ThreadSafeList<NitroxTechType> KnownTechTypes { get; } = new ThreadSafeList<NitroxTechType>();
        
        [DataMember(Order = 2)]
        public ThreadSafeList<NitroxTechType> AnalyzedTechTypes { get; } = new ThreadSafeList<NitroxTechType>();
        
        /// <summary>
        /// Gets or sets the log of story events present in the PDA
        /// </summary>
        [DataMember(Order = 3)]
        public ThreadSafeList<PDALogEntry> PdaLog { get; } = new ThreadSafeList<PDALogEntry>();
        
        /// <summary>
        /// Gets or sets the entries that show up the the PDA's Encyclopedia
        /// </summary>
        [DataMember(Order = 4)]
        public ThreadSafeList<string> EncyclopediaEntries { get; } = new ThreadSafeList<string>();
        
        /// <summary>
        /// The ids of the already scanned entities.
        /// </summary>
        /// <remarks>
        /// In Subnautica, this is a Dictionary, but the value is not used, the only important thing is whether a key is stored or not.
        /// We can therefore use it as a list.
        /// </remarks>
        [DataMember(Order = 5)]
        public ThreadSafeSet<NitroxId> ScannerFragments { get; } = new();
        
        /// <summary>
        /// Partially unlocked PDA entries (e.g. fragments)
        /// </summary>
        [DataMember(Order = 6)]
        public ThreadSafeList<PDAEntry> ScannerPartial { get; } = new();

        /// <summary>
        /// Fully unlocked PDA entries
        /// </summary>
        [DataMember(Order = 7)]
        public ThreadSafeList<NitroxTechType> ScannerComplete { get; } = new();

        public void AddKnownTechType(NitroxTechType techType, List<NitroxTechType> partialTechTypesToRemove)
        {
            ScannerPartial.RemoveAll(entry => partialTechTypesToRemove.Contains(entry.TechType));
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

        public void AddScannerFragment(NitroxId id)
        {
            ScannerFragments.Add(id);
        }

        public void UpdateEntryUnlockedProgress(NitroxTechType techType, int unlockedAmount, bool fullyResearched)
        {
            if (fullyResearched)
            {
                ScannerPartial.RemoveAll(entry => entry.TechType.Equals(techType));
                ScannerComplete.Add(techType);
            }
            else
            {
                lock (ScannerPartial)
                {
                    IEnumerable<PDAEntry> entries = ScannerPartial.Where(e => e.TechType.Equals(techType));
                    if (entries.Any())
                    {
                        entries.First().Unlocked = unlockedAmount;
                    }
                    else
                    {
                        ScannerPartial.Add(new(techType, unlockedAmount));
                    }
                }
            }
        }

        public InitialPDAData GetInitialPDAData()
        {
            return new(KnownTechTypes.ToList(),
                       AnalyzedTechTypes.ToList(),
                       PdaLog.ToList(),
                       EncyclopediaEntries.ToList(),
                       ScannerFragments.ToList(),
                       ScannerPartial.ToList(),
                       ScannerComplete.ToList());
        }
    }
}
