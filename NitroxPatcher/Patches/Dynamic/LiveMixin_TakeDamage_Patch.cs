using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class LiveMixin_TakeDamage_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(LiveMixin);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TakeDamage", BindingFlags.Public | BindingFlags.Instance);
        public static readonly Dictionary<NitroxId, float> ORIGINAL_HEALTH_PER_ENTITY = new Dictionary<NitroxId, float>();
        public static readonly Dictionary<NitroxId, Tuple<LiveMixin, float, Vector3, DamageType, GameObject>> PARAMETER_PER_ENTITY = new Dictionary<NitroxId, Tuple<LiveMixin, float, Vector3, DamageType, GameObject>>();

        public static bool Prefix(LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
        {
            Vehicle vehicle = __instance.GetComponent<Vehicle>();
            SubRoot subRoot = __instance.GetComponent<SubRoot>();
            
            
            if (vehicle != null || subRoot != null && subRoot.isCyclops)
            {
                SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                if (!simulationOwnership.HasAnyLockType(id) && !simulationOwnership.SimulationLockOverrideActive(id))
                {
                    //Log.Debug($"TakeDamage for {__instance.gameObject.name} deteted for damage {originalDamage} without lock for {id}. Will not execute the code");
                    // If the player has no knowledge of an other player simulating the vehicle, try to get the simulation ownership
                    if (!simulationOwnership.OtherPlayerHasAnyLock(id))
                    {
                        Log.Debug($"Send simulation lock request in LiveMixin TakeDamage for {id}");
                        simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT, ResponseRecieved);
                        PARAMETER_PER_ENTITY[id] = new Tuple<LiveMixin, float, Vector3, DamageType, GameObject>(__instance, originalDamage, position, type, dealer);
                    }
                    return false;
                }
                // To prevent damage that happens while docking, we check if dealer is the vehicle that is also docked.
                VehicleDockingBay vehicleDockingBay = __instance.GetComponent<VehicleDockingBay>();
                if (!vehicleDockingBay)
                {
                    vehicleDockingBay = __instance.GetComponentInChildren<VehicleDockingBay>();
                }
                Vehicle dealerVehicle = dealer.GetComponent<Vehicle>();
                Log.Debug($"Dealer: {dealer.gameObject.name} and __instance: {__instance.gameObject.name}; dealerVehicle: {dealerVehicle!=null}, vehicleDockingBay: {vehicleDockingBay!=null}");
                if (vehicleDockingBay && dealerVehicle)
                {
                    if (vehicleDockingBay.GetDockedVehicle() == dealerVehicle || (Vehicle)vehicleDockingBay.ReflectionGet("interpolatingVehicle") == dealerVehicle
                        || (Vehicle)vehicleDockingBay.ReflectionGet("nearbyVehicle") == dealerVehicle)
                    {
                        Log.Debug($"Dealer is vehicle and currently docked or nearby, do not harm it!");
                        return false;
                    }
                }
                //Log.Debug($"TakeDamage for {__instance.gameObject.name} detected for damage {originalDamage} with lock or override for {id}. Will execute code");
                ORIGINAL_HEALTH_PER_ENTITY[id] = __instance.health;
            }
            return true;
        }

        public static void Postfix(LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
        {
            Vehicle vehicle = __instance.GetComponent<Vehicle>();
            SubRoot subRoot = __instance.GetComponent<SubRoot>();
            if (vehicle != null || subRoot != null && subRoot.isCyclops)
            {
                GameObject gameObject = vehicle != null ? vehicle.gameObject : subRoot.gameObject;
                TechType techType = CraftData.GetTechType(gameObject);
                // Send message to other player if LiveMixin is from a vehicle and got the simulation ownership
                SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);

                if (simulationOwnership.HasAnyLockType(id) && ORIGINAL_HEALTH_PER_ENTITY[id] != __instance.health)
                {
                    // This whole code should may be refactored into it's own class if more LiveMixin Entities will be synchronized
                    ushort damagetype = (ushort)type;
                    NitroxId dealerId = NitroxEntity.GetId(dealer);
                    LiveMixinHealthChanged packet = new LiveMixinHealthChanged(techType.ToDto(), id, -originalDamage, position.ToDto(), damagetype, Optional.Of<NitroxId>(dealerId), __instance.health);
                    NitroxServiceLocator.LocateService<IMultiplayerSession>().Send(packet);
                }
                ORIGINAL_HEALTH_PER_ENTITY.Remove(id);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }

        public static void ResponseRecieved(NitroxId id, bool lockAquired)
        {
            if (lockAquired)
            {
                Tuple<LiveMixin, float, Vector3, DamageType, GameObject> parameter = null;
                PARAMETER_PER_ENTITY.TryGetValue(id, out parameter);
                if (parameter != null)
                {
                    parameter.Item1.TakeDamage(parameter.Item2, parameter.Item3, parameter.Item4, parameter.Item5);
                    PARAMETER_PER_ENTITY.Remove(id);
                }
                else
                {
                    Log.Warn($"Got a simulation lock response for entity {id} but did not find the corresponding parameter. This should not happen!");
                }
            }
        }
    }
}
