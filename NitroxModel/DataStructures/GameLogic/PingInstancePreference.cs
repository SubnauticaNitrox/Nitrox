using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable, ProtoContract]
public class PingInstancePreference
{
    [ProtoMember(1)]
    public int Color { get; set; }

    [ProtoMember(2)]
    public bool Visible { get; set; }

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
