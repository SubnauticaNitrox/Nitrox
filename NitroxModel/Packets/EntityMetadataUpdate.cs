using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class EntityMetadataUpdate : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual EntityMetadata NewValue { get; protected set; }

        public EntityMetadataUpdate() { }

        public EntityMetadataUpdate(NitroxId id, EntityMetadata newValue)
        {
            Id = id;
            NewValue = newValue;
        }
    }
}
