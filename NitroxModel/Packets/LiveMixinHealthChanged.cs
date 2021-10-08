using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;

namespace NitroxModel.Packets
{
    [Serializable]
    [ProtoContract]
    public class LiveMixinHealthChanged : Packet
    {
        public Optional<DamageTakenData> DamageTakenData;
        public NitroxTechType TechType { get; }
        public NitroxId Id { get; set; }
        public float LifeChanged { get; set; }
        public float TotalHealth { get; }

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
