using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    public class StarshipDoorMetadata : EntityMetadata
    {
        [DataMember(Order = 1)]
        public bool DoorLocked { get; }

        [DataMember(Order = 2)]
        public bool DoorOpen { get; }

        [IgnoreConstructor]
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
