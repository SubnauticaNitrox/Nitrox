using System;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic.Creatures.Actions;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CreatureActionChanged : Packet
    {
        public string Guid { get; }
        public SerializableCreatureAction NewAction { get; }

        public CreatureActionChanged(string guid, SerializableCreatureAction newAction)
        {
            Guid = guid;
            NewAction = newAction;
        }
    }
}
