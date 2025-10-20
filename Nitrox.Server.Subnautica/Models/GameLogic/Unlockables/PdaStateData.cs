using System.Runtime.Serialization;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

/// <summary>
///     Data container for everything Subnautica PDA related. Behavior should go in <see cref="PdaManager" />.
/// </summary>
[DataContract]
internal sealed class PdaStateData
{
    /// <summary>
    ///     Gets or sets the KnownTech construct which powers the popup shown to the user when a new TechType is discovered
    ///     ("New Creature Discovered!")
    ///     The KnownTech construct uses both <see cref='KnownTechEntryAdd.EntryCategory.KNOWN'>KnownTech.knownTech</see> and
    ///     <see cref='KnownTechEntryAdd.EntryCategory.ANALYZED'>KnownTech.analyzedTech</see>
    /// </summary>
    [DataMember(Order = 1)]
    public ThreadSafeList<NitroxTechType> KnownTechTypes { get; } = [];

    [DataMember(Order = 2)]
    public ThreadSafeList<NitroxTechType> AnalyzedTechTypes { get; } = [];

    /// <summary>
    ///     Gets or sets the log of story events present in the PDA
    /// </summary>
    [DataMember(Order = 3)]
    public ThreadSafeList<PDALogEntry> PdaLog { get; } = [];

    /// <summary>
    ///     Gets or sets the entries that show up the the PDA's Encyclopedia
    /// </summary>
    [DataMember(Order = 4)]
    public ThreadSafeList<string> EncyclopediaEntries { get; } = [];

    /// <summary>
    ///     The ids of the already scanned entities.
    /// </summary>
    /// <remarks>
    ///     In Subnautica, this is a Dictionary, but the value is not used, the only important thing is whether a key is stored
    ///     or not.
    ///     We can therefore use it as a list.
    /// </remarks>
    [DataMember(Order = 5)]
    public ThreadSafeSet<NitroxId> ScannerFragments { get; } = [];

    /// <summary>
    ///     Partially unlocked PDA entries (e.g. fragments)
    /// </summary>
    [DataMember(Order = 6)]
    public ThreadSafeList<PDAEntry> ScannerPartial { get; } = [];

    /// <summary>
    ///     Fully unlocked PDA entries
    /// </summary>
    [DataMember(Order = 7)]
    public ThreadSafeList<NitroxTechType> ScannerComplete { get; } = [];
}
