using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.Packets
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
