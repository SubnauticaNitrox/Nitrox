using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class ScannerRoomScanStateChanged(NitroxId mapRoomId, ScannerRoomScanState canonicalState) : Packet
{
    public NitroxId MapRoomId { get; } = mapRoomId;
    public ScannerRoomScanState CanonicalState { get; } = canonicalState;
}
