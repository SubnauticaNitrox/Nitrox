using ProtoBufNet;

namespace NitroxServer.UnityStubs;

[ProtoContract]
public class ComponentHeader
{
    [ProtoMember(1)]
    public string TypeName
    {
        get;
        set;
    }

    [ProtoMember(2)]
    public bool IsEnabled
    {
        get;
        set;
    }

    public void Reset()
    {
        TypeName = null;
        IsEnabled = false;
    }

    public override string ToString()
    {
        return string.Format("(TypeName={0}, IsEnabled={1})", TypeName, IsEnabled);
    }
}
