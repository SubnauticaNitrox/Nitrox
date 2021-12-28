using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    [ProtoContract]
    public class LiveMixinHealthChanged : Packet
    {
        [Index(0)]
        public virtual Optional<DamageTakenData> DamageTakenData { get; protected set; }
        [Index(1)]
        public virtual NitroxTechType TechType { get; protected set; }
        [Index(2)]
        public virtual NitroxId Id { get; protected set; }
        [Index(3)]
        public virtual float LifeChanged { get; protected set; }
        [Index(4)]
        public virtual float TotalHealth { get; protected set; }

        public LiveMixinHealthChanged() { }

        public LiveMixinHealthChanged(NitroxTechType techType, NitroxId id, float lifeChanged, float totalHealth)
        {
            TechType = techType;
            Id = id;
            LifeChanged = lifeChanged;
            TotalHealth = totalHealth;
            DamageTakenData = Optional.Empty;
        }

        public LiveMixinHealthChanged(NitroxTechType techType, NitroxId id, float lifeChanged, float totalHealth, NitroxVector3 position, ushort damageType, Optional<NitroxId> dealerId)
        {
            TechType = techType;
            Id = id;
            LifeChanged = lifeChanged;
            TotalHealth = totalHealth;
            DamageTakenData = new DamageTakenData
            {
                Position = position,
                DamageType = damageType,
                DealerId = dealerId
            };
        }
    }
}
