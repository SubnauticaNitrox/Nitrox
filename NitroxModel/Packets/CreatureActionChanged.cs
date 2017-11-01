using System;
using NitroxModel.GameLogic.Creatures.Actions;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CreatureActionChanged : Packet
    {
        public String Guid { get; }
        public SerializableCreatureAction NewAction { get; }

        public CreatureActionChanged(String guid, SerializableCreatureAction newAction)
        {
            this.Guid = guid;
            this.NewAction = newAction;
        }
    }
}
