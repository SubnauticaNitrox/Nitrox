using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using ProtoBufNet;

namespace Nitrox.Model.Packets
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

        public LiveMixinHealthChanged(NitroxTechType techType, NitroxId id, float lifeChanged, NitroxVector3 position, ushort damageType, Optional<NitroxId> dealerId, float totalHealth)
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
            return $"[LiveMixinHealthChanged packet: TechType: {TechType}, Id: {Id}, lifeChanged: {LifeChanged}, totalHealth {TotalHealth}, {DamageTakenData}]";
        }
    }
}
