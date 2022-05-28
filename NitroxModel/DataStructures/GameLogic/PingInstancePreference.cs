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

    public override bool Equals(object obj)
    {
        return obj is PingInstancePreference otherInstance && Color == otherInstance.Color && Visible == otherInstance.Visible;
    }

    public override int GetHashCode()
    {
        int hashCode = 295593209;
        hashCode = hashCode * -1521134295 + Color.GetHashCode();
        hashCode = hashCode * -1521134295 + Visible.GetHashCode();
        return hashCode;
    }
}
