using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class ScannerRoomScanStateChangeRequest(NitroxId mapRoomId, NitroxTechType? desiredTechType) : Packet
{
    public NitroxId MapRoomId { get; } = mapRoomId;
    public NitroxTechType? DesiredTechType { get; } = desiredTechType;
}
