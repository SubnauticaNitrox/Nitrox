using System;
using NitroxModel.DataStructures.GameLogic.Creatures.Actions;

namespace NitroxModel.Packets
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
