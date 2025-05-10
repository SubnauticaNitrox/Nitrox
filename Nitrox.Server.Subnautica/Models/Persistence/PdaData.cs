using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Persistence;

[DataContract]
internal record PdaData
{
    /// <summary>
    /// Gets or sets the KnownTech construct which powers the popup shown to the user when a new TechType is discovered ("New Creature Discovered!")
    /// The KnownTech construct uses both <see cref='KnownTechEntryAdd.EntryCategory.KNOWN'>KnownTech.knownTech</see> and <see cref='KnownTechEntryAdd.EntryCategory.ANALYZED'>KnownTech.analyzedTech</see>
    /// </summary>
    [DataMember(Order = 1)]
    public ThreadSafeList<NitroxTechType> KnownTechTypes { get; } = [];

    [DataMember(Order = 2)]
    public ThreadSafeList<NitroxTechType> AnalyzedTechTypes { get; } = [];

    /// <summary>
    /// Gets or sets the log of story events present in the PDA.
    /// </summary>
    [DataMember(Order = 3)]
    public ThreadSafeList<PdaLogEntry> PdaLog { get; } = [];

    /// <summary>
    /// Gets or sets the entries that show up in the PDAs Encyclopedia.
    /// </summary>
    [DataMember(Order = 4)]
    public ThreadSafeList<string> EncyclopediaEntries { get; } = [];

    /// <summary>
    /// The ids of the already scanned entities.
    /// </summary>
    /// <remarks>
    /// In Subnautica, this is a Dictionary, but the value is not used, the only important thing is whether a key is stored or not.
    /// We can therefore use it as a list.
    /// </remarks>
    [DataMember(Order = 5)]
    public ThreadSafeSet<NitroxId> ScannerFragments { get; } = [];

    /// <summary>
    /// Partially unlocked PDA entries (e.g. fragments)
    /// </summary>
    [DataMember(Order = 6)]
    public ThreadSafeList<PDAEntry> ScannerPartial { get; } = [];

    /// <summary>
    /// Fully unlocked PDA entries
    /// </summary>
    [DataMember(Order = 7)]
    public ThreadSafeList<NitroxTechType> ScannerComplete { get; } = [];

    public void AddKnownTechType(NitroxTechType techType, List<NitroxTechType> partialTechTypesToRemove)
    {
        ScannerPartial.RemoveAll(entry => partialTechTypesToRemove.Contains(entry.TechType));
        if (!KnownTechTypes.Contains(techType))
        {
            KnownTechTypes.Add(techType);
        }
        else
        {
            // TODO: USE DATABASE - handle as constraint
            // logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the KnownTechTypes: [{techType.Name}]");
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
            // TODO: USE DATABASE - handle as constraint
            // logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the AnalyzedTechTypes: [{techType.Name}]");
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
            // TODO: USE DATABASE - handle as constraint
            // logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the EncyclopediaEntries: [{entry}]");
        }
    }

    public void AddPdaLogEntry(PdaLogEntry entry)
    {
        if (PdaLog.Contains(entry))
        {
            // TODO: USE DATABASE - handle as constraint
            // logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the PDALog: [{entry.Key}]");
            return;
        }
        PdaLog.Add(entry);
    }

    public void AddScannerFragment(NitroxId id)
    {
        ScannerFragments.Add(id);
    }

    public void UpdateEntryUnlockedProgress(NitroxTechType techType, int unlockedAmount, bool fullyResearched)
    {
        if (fullyResearched)
        {
            ScannerPartial.RemoveAllFast(techType, (entry, tt) => entry.TechType.Equals(tt));
            ScannerComplete.Add(techType);
        }
        else
        {
            PDAEntry entry = ScannerPartial.FirstOrDefault(e => e.TechType.Equals(techType));
            if (entry != null)
            {
                entry.Unlocked = unlockedAmount;
            }
            else
            {
                ScannerPartial.Add(new(techType, unlockedAmount));
            }
        }
    }

    // TODO: Send this for initial sync.
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
