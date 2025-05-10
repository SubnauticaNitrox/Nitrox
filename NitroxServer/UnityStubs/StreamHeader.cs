using ProtoBufNet;

namespace NitroxServer.UnityStubs;

[ProtoContract]
public class StreamHeader
{
    [ProtoMember(1)]
    public int Signature
    {
        get;
        set;
    }

    [ProtoMember(2)]
    public int Version
    {
        get;
        set;
    }

    public void Reset()
    {
        Signature = 0;
        Version = 0;
    }

    public override string ToString()
    {
        return string.Format("(UniqueIdentifier={0}, Version={1})", Signature, Version);
    }
}
