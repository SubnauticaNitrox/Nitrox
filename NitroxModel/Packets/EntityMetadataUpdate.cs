using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EntityMetadataUpdate : Packet
    {
        public NitroxId Id { get; }

        public EntityMetadata NewValue { get; }

        public EntityMetadataUpdate(NitroxId id, EntityMetadata newValue)
        {
            Id = id;
            NewValue = newValue;
        }

        public override string ToString()
        {
            return $"[EntityMetadataUpdate id={Id} NewValue={NewValue} ]";
        }
    }
}
