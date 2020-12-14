using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    [Serializable]
    public class LiveMixinHealthChanged : Packet
    {
        public NitroxTechType TechType { get; }
        public NitroxId Id { get; }
        public float LifeChanged { get; }
        public float TotalHealth { get; }
        public Optional<DamageTakenData> DamageTakenData { get; }

        public LiveMixinHealthChanged(NitroxTechType techType, NitroxId id, float lifeChanged, float totalHealth, NitroxVector3 position, ushort damageType, Optional<NitroxId> dealerId)
        {
            TechType = techType;
            Id = id;
            TotalHealth = totalHealth;
            LifeChanged = lifeChanged;
            DamageTakenData = new DamageTakenData
            {
                Position = position,
                DamageType = damageType,
                DealerId = dealerId
            };
        }

        public LiveMixinHealthChanged(NitroxTechType techType, NitroxId id, float lifeChanged, float totalHealth)
        {
            TechType = techType;
            Id = id;
            TotalHealth = totalHealth;
            LifeChanged = lifeChanged;
            DamageTakenData = Optional.Empty;
        }

        public override string ToString()
        {
            return $"[LiveMixinHealthChanged - TechType: {TechType}, Id: {Id}, LifeChanged: {LifeChanged}, TotalHealth {TotalHealth}, DamageTakenData: {DamageTakenData}]";
        }
    }
}
