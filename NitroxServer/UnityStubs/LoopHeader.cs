using ProtoBufNet;

namespace NitroxServer.UnityStubs;

[ProtoContract]
public class LoopHeader
{
    [ProtoMember(1)]
    public int Count
    {
        get;
        set;
    }

    public void Reset()
    {
        Count = 0;
    }

    public override string ToString()
    {
        return string.Format("(Count={0})", Count);
    }
}
