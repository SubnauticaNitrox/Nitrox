using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic.Creatures.Actions;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CreatureActionChanged : Packet
    {
        public NitroxId Id { get; }
        public SerializableCreatureAction NewAction { get; }

        public CreatureActionChanged(NitroxId id, SerializableCreatureAction newAction)
        {
            Id = id;
            NewAction = newAction;
        }

        public override string ToString()
        {
            return $"[CreatureActionChanged - Id: {Id}, NewAction: {NewAction}]";
        }
    }
}
