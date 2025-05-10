using NitroxModel.DataStructures;
using ProtoBufNet;

namespace NitroxServer.UnityStubs;

[ProtoContract]
public class CellHeaderEx
{
    public override string ToString()
    {
        return string.Format("(cellId={0}, level={1}, dataLength={2}, legacyDataLength={3}, waiterDataLength={4})", new object[]
        {
            CellId,
            Level,
            DataLength,
            LegacyDataLength,
            WaiterDataLength
        });
    }

    [ProtoMember(1)]
    public NitroxInt3 CellId;

    [ProtoMember(2)]
    public int Level;

    [ProtoMember(3)]
    public int DataLength;

    [ProtoMember(4)]
    public int LegacyDataLength;

    [ProtoMember(5)]
    public int WaiterDataLength;

    // There's no point in spawning allowSpawnRestrictions as SpawnRestrictionEnforcer doesn't load any restrictions
}
