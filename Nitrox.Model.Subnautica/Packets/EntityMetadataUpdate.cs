using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.Subnautica.Packets
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
    }
}
