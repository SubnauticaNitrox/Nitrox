using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public sealed record CreatureActionChanged : Packet
{
    public NitroxId CreatureId { get; }
    public string CreatureActionType { get; }

    public CreatureActionChanged(NitroxId creatureId, string creatureActionType)
    {
        CreatureId = creatureId;
        CreatureActionType = creatureActionType;
    }
}
