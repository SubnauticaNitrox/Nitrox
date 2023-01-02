using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[DataContract]
public class PingInstancePreference
{
    [DataMember(Order = 1)]
    public int Color { get; set; }

    [DataMember(Order = 2)]
    public bool Visible { get; set; }

    [IgnoreConstructor]
    protected PingInstancePreference()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PingInstancePreference(int color, bool visible)
    {
        Color = color;
        Visible = visible;
    }
}
