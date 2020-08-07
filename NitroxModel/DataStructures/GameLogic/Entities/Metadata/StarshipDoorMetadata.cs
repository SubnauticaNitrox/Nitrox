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

        protected StarshipDoorMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public StarshipDoorMetadata(bool doorLocked)
        {
            DoorLocked = doorLocked;
        }
    }
}
