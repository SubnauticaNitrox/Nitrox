using System;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EntitySpawnedByClient : Packet
    {
        public WorldEntity Entity { get; }

        public EntitySpawnedByClient(WorldEntity entity)
        {
            Entity = entity;
        }
    }
}
