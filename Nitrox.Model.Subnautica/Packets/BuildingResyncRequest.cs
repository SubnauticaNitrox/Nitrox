using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class BuildingResyncRequest(NitroxId? entityId = null, bool resyncEverything = true) : Packet
{
    public NitroxId? EntityId { get; } = entityId;
    public bool ResyncEverything { get; } = resyncEverything;
}
