using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets;

[Serializable]
public class EntitySpawnedByClient : Packet
{
    public Entity Entity { get; }
    public bool RequireRespawn { get; }

    public EntitySpawnedByClient(Entity entity, bool requireRespawn = false)
    {
        Entity = entity;
        RequireRespawn = requireRespawn;
    }
}
