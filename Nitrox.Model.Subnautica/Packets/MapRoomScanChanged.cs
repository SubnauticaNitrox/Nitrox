using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class MapRoomScanChanged : Packet
{
    public NitroxId MapRoomId { get; }
    public string TechType { get; }

    public MapRoomScanChanged(NitroxId mapRoomId, string techType)
    {
        MapRoomId = mapRoomId;
        TechType = techType;
    }
}
