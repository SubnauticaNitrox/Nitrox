using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [ZeroFormattable]
    [ProtoContract]
    public class StarshipDoorMetadata : EntityMetadata
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual bool DoorLocked { get; protected set; }
        [Index(1)]
        [ProtoMember(2)]
        public virtual bool DoorOpen { get; protected set; }

        protected StarshipDoorMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public StarshipDoorMetadata(bool doorLocked, bool doorOpen)
        {
            DoorLocked = doorLocked;
            DoorOpen = doorOpen;
        }

        public override string ToString()
        {
            return $"[StarshipDoorMetadata DoorLocked: {DoorLocked} DoorOpen: {DoorOpen}]";
        }
    }
}
