using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EntitySpawnedByClient : Packet
    {
        public Entity Entity { get; }

        public EntitySpawnedByClient(Entity entity)
        {
            Entity = entity;
        }
    }
}
