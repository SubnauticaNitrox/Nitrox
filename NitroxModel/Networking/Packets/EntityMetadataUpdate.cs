using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record EntityMetadataUpdate : Packet
    {
        public NitroxId Id { get; }

        public EntityMetadata NewValue { get; }

        public EntityMetadataUpdate(NitroxId id, EntityMetadata newValue)
        {
            Id = id;
            NewValue = newValue;
        }
    }
}
