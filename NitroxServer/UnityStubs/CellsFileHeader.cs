using ProtoBufNet;

namespace NitroxServer.UnityStubs;

[ProtoContract]
public class CellsFileHeader
{
    public override string ToString()
    {
        return string.Format("(version={0}, numCells={1})", Version, NumCells);
    }

    [ProtoMember(1)]
    public int Version;

    [ProtoMember(2)]
    public int NumCells;
}
