using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable, DataContract]
public class PDAEntry
{
    [DataMember(Order = 1)]
    public NitroxTechType TechType { get; set; }

    [DataMember(Order = 2)]
    public int Unlocked { get; set; }

    [IgnoreConstructor]
    protected PDAEntry()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PDAEntry(NitroxTechType techType, int unlocked)
    {
        TechType = techType;
        Unlocked = unlocked;
    }
}
