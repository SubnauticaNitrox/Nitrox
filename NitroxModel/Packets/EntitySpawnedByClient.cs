using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets;

[Serializable]
public class EntitySpawnedByClient : Packet
{
    public Entity Entity { get; }
    public bool RequireRespawn { get; }
    public bool RequireSimulation { get; }

    public EntitySpawnedByClient(Entity entity, bool requireRespawn = false, bool requireSimulation = true)
    {
        Entity = entity;
        RequireRespawn = requireRespawn;
        RequireSimulation = requireSimulation;
    }
}
