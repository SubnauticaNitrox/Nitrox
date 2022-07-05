using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class LiveMixinManager
    {

        private readonly IMultiplayerSession multiplayerSession;
        private readonly SimulationOwnership simulationOwnership;

        private bool processingRemoteHealthChange = false;

        public LiveMixinManager(IMultiplayerSession multiplayerSession, SimulationOwnership simulationOwnership)
        {
            this.multiplayerSession = multiplayerSession;
            this.simulationOwnership = simulationOwnership;
        }

        // Currently, we only apply live mixin updates to vehicles as there is more work to implement
        // damage for regular entities like fish.
        public bool IsWhitelistedUpdateType(LiveMixin entity)
        {
            Vehicle vehicle = entity.GetComponent<Vehicle>();
            SubRoot subRoot = entity.GetComponent<SubRoot>();

            return (vehicle != null || (subRoot != null && subRoot.isCyclops));
        }

        public bool ShouldApplyNextHealthUpdate(LiveMixin reciever, GameObject dealer = null)
        {
            NitroxId id = NitroxEntity.GetId(reciever.gameObject);

            if (!simulationOwnership.HasAnyLockType(id) && !processingRemoteHealthChange)
            {
                return false;
            }

            // Check to see if this health change is caused by docked vehicle collisions.  If so, we don't want to apply it.
            if (dealer)
            {
                Vehicle dealerVehicle = dealer.GetComponent<Vehicle>();
                VehicleDockingBay vehicleDockingBay = reciever.GetComponentInChildren<VehicleDockingBay>();

                if (vehicleDockingBay && dealerVehicle)
                {
                    if (vehicleDockingBay.GetDockedVehicle() == dealerVehicle || vehicleDockingBay.interpolatingVehicle == dealerVehicle
                        || vehicleDockingBay.nearbyVehicle == dealerVehicle)
                    {
                        Log.Debug($"Dealer {dealer} is vehicle and currently docked or nearby {reciever}, do not harm it!");
                        return false;
                    }
                }
            }

            return true;
        }

        public void ProcessRemoteHealthChange(NitroxId id, float LifeChanged, Optional<DamageTakenData> opDamageTakenData, float totalHealth)
        {
            if (simulationOwnership.HasAnyLockType(id))
            {
                Log.Error($"Got LiveMixin change health for {id} but we have the simulation already. This should not happen!");
                return;
            }

            processingRemoteHealthChange = true;

            LiveMixin liveMixin = NitroxEntity.RequireObjectFrom(id).GetComponent<LiveMixin>();

            if (LifeChanged < 0)
            {
                DamageTakenData damageTakenData = opDamageTakenData.OrNull();
                Optional<GameObject> opDealer = damageTakenData.DealerId.HasValue ? NitroxEntity.GetObjectFrom(damageTakenData.DealerId.Value) : Optional.Empty;
                GameObject dealer = opDealer.HasValue ? opDealer.Value : null;
                if (!dealer && damageTakenData.DealerId.HasValue)
                {
                    Log.Warn($"Could not find entity {damageTakenData.DealerId.Value} for damage calculation. This could lead to problems.");
                }
                liveMixin.TakeDamage(-LifeChanged, damageTakenData.Position.ToUnity(), (DamageType)damageTakenData.DamageType, dealer);
            }
            else
            {
                liveMixin.AddHealth(LifeChanged);
            }

            processingRemoteHealthChange = false;

            // Check if the health calculated by the game is the same as the calculated damage from the simulator
            if (liveMixin.health != totalHealth)
            {
                Log.Warn($"Calculated health and send health for {id} do not align (Calculated: {liveMixin.health}, send:{totalHealth}). This will be correted but should be investigated");
                liveMixin.health = totalHealth;
            }
        }

        public void BroadcastTakeDamage(TechType techType, NitroxId id, float originalDamage, Vector3 position, DamageType damageType, Optional<NitroxId> dealerId, float totalHealth)
        {
            LiveMixinHealthChanged packet = new LiveMixinHealthChanged(techType.ToDto(), id, -originalDamage, totalHealth, position.ToDto(), (ushort)damageType, dealerId);
            multiplayerSession.Send(packet);
        }

        public void BroadcastAddHealth(TechType techType, NitroxId id, float healthAdded, float totalHealth)
        {
            LiveMixinHealthChanged packet = new LiveMixinHealthChanged(techType.ToDto(), id, healthAdded, totalHealth);
            multiplayerSession.Send(packet);
        }
    }
}
