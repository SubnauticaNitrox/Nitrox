using System.Collections.Generic;
using System.Runtime.Serialization;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

/// <summary>
///     Data container for everything Subnautica PDA related. Behavior should go in <see cref="PdaManager" />.
/// </summary>
/// <remarks>
///     Important: Keep <see cref="GetFullCopy" /> in sync with any code changes!
/// </remarks>
[DataContract]
internal sealed record PdaStateData
{
    /// <summary>
    ///     Gets or sets the KnownTech construct which powers the popup shown to the user when a new TechType is discovered
    ///     ("New Creature Discovered!")
    ///     The KnownTech construct uses both <see cref='KnownTechEntryAdd.EntryCategory.KNOWN'>KnownTech.knownTech</see> and
    ///     <see cref='KnownTechEntryAdd.EntryCategory.ANALYZED'>KnownTech.analyzedTech</see>
    /// </summary>
    [DataMember(Order = 1)]
    public List<NitroxTechType> KnownTechTypes { get; } = [];

    [DataMember(Order = 2)]
    public List<NitroxTechType> AnalyzedTechTypes { get; } = [];

    /// <summary>
    ///     Gets or sets the log of story events present in the PDA
    /// </summary>
    [DataMember(Order = 3)]
    public List<PDALogEntry> PdaLog { get; } = [];

    /// <summary>
    ///     Gets or sets the entries that show up the the PDA's Encyclopedia
    /// </summary>
    [DataMember(Order = 4)]
    public List<string> EncyclopediaEntries { get; } = [];

    /// <summary>
    ///     The ids of the already scanned entities.
    /// </summary>
    /// <remarks>
    ///     In Subnautica, this is a Dictionary, but the value is not used, the only important thing is whether a key is stored
    ///     or not.
    ///     We can therefore use it as a list.
    /// </remarks>
    [DataMember(Order = 5)]
    public HashSet<NitroxId> ScannerFragments { get; } = [];

    /// <summary>
    ///     Partially unlocked PDA entries (e.g. fragments)
    /// </summary>
    [DataMember(Order = 6)]
    public List<PDAEntry> ScannerPartial { get; } = [];

    /// <summary>
    ///     Fully unlocked PDA entries
    /// </summary>
    [DataMember(Order = 7)]
    public List<NitroxTechType> ScannerComplete { get; } = [];

    /// <summary>
    ///     Gets a full copy of the entire <see cref="PdaStateData" /> which allows for thread safe access.
    /// </summary>
    public PdaStateData GetFullCopy()
    {
        PdaStateData copy = new();
        copy.KnownTechTypes.AddRange(KnownTechTypes);
        copy.AnalyzedTechTypes.AddRange(AnalyzedTechTypes);
        copy.PdaLog.AddRange(PdaLog);
        copy.EncyclopediaEntries.AddRange(EncyclopediaEntries);
        foreach (NitroxId fragment in ScannerFragments)
        {
            copy.ScannerFragments.Add(fragment);
        }
        copy.ScannerPartial.AddRange(ScannerPartial);
        copy.ScannerComplete.AddRange(ScannerComplete);
        return copy;
    }
}
