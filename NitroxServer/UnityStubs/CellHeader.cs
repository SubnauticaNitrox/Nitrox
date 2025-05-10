using NitroxModel.DataStructures;
using ProtoBufNet;

namespace NitroxServer.UnityStubs;

[ProtoContract]
public class CellHeader
{
    public override string ToString()
    {
        return $"(cellId={CellId}, level={Level})";
    }

    [ProtoMember(1)]
    public NitroxInt3 CellId;

    [ProtoMember(2)]
    public int Level;
}
