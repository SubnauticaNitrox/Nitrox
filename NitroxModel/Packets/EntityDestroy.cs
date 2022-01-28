using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EntityDestroy : Packet
    {
        public NitroxId EntityId { get; }

        public EntityDestroy(NitroxId entityId)
        {
            EntityId = entityId;
        }
    }
}
