using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class LiveMixinManager
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly SimulationOwnership simulationOwnership;
        private readonly Dictionary<NitroxId, Tuple<float, float>> outstandingChangeHealth = new Dictionary<NitroxId, Tuple<float, float>>();
        public LiveMixinManager(IMultiplayerSession multiplayerSession, SimulationOwnership simulationOwnership)
        {
            this.multiplayerSession = multiplayerSession;
            this.simulationOwnership = simulationOwnership;
        }
        /// <summary>
        /// Indicates if a LiveMixin object should execute TakeDamage or HealthBack method
        /// </summary>
        /// <param name="reciever">The one who takes damage or gets health back</param>
        /// <param name="healthChange">The amount of health it gets back or damage dealt</param>
        /// <param name="dealer">Damage dealer</param>
        /// <returns>Tuple where the first item indicates execution and the second indicates if the player has the ownership of the reciever GameObject (if vehicle)</returns>
        public Tuple<bool, bool> ShouldExecute(LiveMixin reciever, float healthChange, GameObject dealer)
        {
            Vehicle vehicle = reciever.GetComponent<Vehicle>();
            SubRoot subRoot = reciever.GetComponent<SubRoot>();


            if (vehicle != null || subRoot != null && subRoot.isCyclops)
            {
                NitroxId id = NitroxEntity.GetId(reciever.gameObject);
                
                bool isOutstandingPresent = outstandingChangeHealth.TryGetValue(id, out Tuple<float, float> healthChangeAndTotal) &&
                    healthChangeAndTotal.Item1 == healthChange && healthChangeAndTotal.Item2 == reciever.health;
                bool hasOwnership = simulationOwnership.HasAnyLockType(id);

                // We either have a lock or an outstanding health change with the same health change and current total health when we execute the code
                if (!hasOwnership && !isOutstandingPresent)
                {
                    return new Tuple<bool, bool>(false, false);
                }

                // Remove the outstandingHealthChange if present.
                if (isOutstandingPresent)
                {
                    outstandingChangeHealth.Remove(id);
                }

                if (healthChange > 0)
                {
                    return new Tuple<bool, bool>(true, hasOwnership);
                }
                // To prevent damage that happens while docking, we check if dealer is the vehicle that is also docked.
                VehicleDockingBay vehicleDockingBay = reciever.GetComponent<VehicleDockingBay>();
                if (!vehicleDockingBay)
                {
                    vehicleDockingBay = reciever.GetComponentInChildren<VehicleDockingBay>();
                }
                Vehicle dealerVehicle = dealer.GetComponent<Vehicle>();
                if (vehicleDockingBay && dealerVehicle)
                {
                    if (vehicleDockingBay.GetDockedVehicle() == dealerVehicle || (Vehicle)vehicleDockingBay.ReflectionGet("interpolatingVehicle") == dealerVehicle
                        || (Vehicle)vehicleDockingBay.ReflectionGet("nearbyVehicle") == dealerVehicle)
                    {
                        Log.Debug($"Dealer {dealer} is vehicle and currently docked or nearby {reciever}, do not harm it!");
                        return new Tuple<bool, bool>(false, false);
                    }
                }
                return new Tuple<bool, bool>(true, hasOwnership);
            }
            return new Tuple<bool, bool>(true, false);
        }

        public void ProcessRemoteHealthChange(NitroxId id, float LifeChanged, Optional<DamageTakenData> opDamageTakenData, float totalHealth)
        {
            if (simulationOwnership.HasAnyLockType(id))
            {
                Log.Error($"Got LiveMixin change health for {id} but we have the simulation already. This should not happen!");
                return;
            }
            LiveMixin liveMixin = NitroxEntity.RequireObjectFrom(id).GetComponent<LiveMixin>();
            // For remote processing, we add an outstanding health change that makes it possible to pass execution
            outstandingChangeHealth.Add(id, new Tuple<float, float>(LifeChanged, liveMixin.health));
            if (LifeChanged < 0)
            {
                DamageTakenData damageTakenData = opDamageTakenData.OrElse(null);
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

            // Check if the health calculated by the game is the same as the calculated damage from the simulator
            if (liveMixin.health != totalHealth)
            {
                Log.Warn($"Calculated health and send health for {id} do not align (Calculated: {liveMixin.health}, send:{totalHealth}). This will be correted but should be investigated");
                liveMixin.health = totalHealth;
            }
        }

        public void BroadcastTakeDamage(TechType techType, NitroxId id, float originalDamage, Vector3 position, DamageType damageType, Optional<NitroxId> dealerId, float totalHealth)
        {
            LiveMixinHealthChanged packet = new LiveMixinHealthChanged(techType.ToDto(), id, -originalDamage, position.ToDto(), (ushort)damageType, dealerId, totalHealth);
            multiplayerSession.Send(packet);
        }

        public void BroadcastAddHealth(TechType techType, NitroxId id, float healthAdded, float totalHealth)
        {
            LiveMixinHealthChanged packet = new LiveMixinHealthChanged(techType.ToDto(), id, healthAdded, totalHealth);
            multiplayerSession.Send(packet);
        }
    }
}
