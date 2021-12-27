using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic.Creatures.Actions;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class CreatureActionChanged : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual SerializableCreatureAction NewAction { get; protected set; }

        private CreatureActionChanged() { }

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
