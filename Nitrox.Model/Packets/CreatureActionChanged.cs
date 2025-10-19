using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public sealed class CreatureActionChanged : Packet
{
    public NitroxId CreatureId { get; }
    public string CreatureActionType { get; }

    public CreatureActionChanged(NitroxId creatureId, string creatureActionType)
    {
        CreatureId = creatureId;
        CreatureActionType = creatureActionType;
    }
}
