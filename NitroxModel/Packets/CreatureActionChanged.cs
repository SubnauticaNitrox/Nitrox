using System;
using NitroxModel.GameLogic.Creatures.Actions;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CreatureActionChanged : AuthenticatedPacket
    {
        public String Guid { get; }
        public SerializableCreatureAction NewAction { get; }

        public CreatureActionChanged(String guid, SerializableCreatureAction newAction, String playerId) : base(playerId)
        {
            this.Guid = guid;
            this.NewAction = newAction;
        }
    }
}
