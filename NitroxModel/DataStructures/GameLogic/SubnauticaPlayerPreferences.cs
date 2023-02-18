using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable, DataContract]
public class SubnauticaPlayerPreferences
{
    [DataMember(Order = 1)]
    public Dictionary<string, PingInstancePreference> PingPreferences { get; set; } = new();

    /// <remarks>
    /// The int type refers to a TechType.
    /// </remarks>
    [DataMember(Order = 2)]
    public List<int> PinnedTechTypes { get; set; } = new();

    [IgnoreConstructor]
    protected SubnauticaPlayerPreferences()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public SubnauticaPlayerPreferences(Dictionary<string, PingInstancePreference> pingPreferences, List<int> pinnedTechTypes)
    {
        PingPreferences = pingPreferences;
        PinnedTechTypes = pinnedTechTypes;
    }
}
