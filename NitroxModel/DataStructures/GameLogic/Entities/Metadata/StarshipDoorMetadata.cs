using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract]
    public class StarshipDoorMetadata : EntityMetadata
    {
        [ProtoMember(1)]
        public bool DoorLocked { get; }
        [ProtoMember(2)]
        public bool DoorOpen { get; }

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
