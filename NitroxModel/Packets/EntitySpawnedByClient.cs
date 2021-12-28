using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class EntitySpawnedByClient : Packet
    {
        [Index(0)]
        public virtual Entity Entity { get; protected set; }

        public EntitySpawnedByClient() { }

        public EntitySpawnedByClient(Entity entity)
        {
            Entity = entity;
        }
    }
}
