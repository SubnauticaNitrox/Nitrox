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
                    return false;
                }
                // To prevent damage that happens while docking, we check if dealer is the vehicle that is also docked.
                VehicleDockingBay vehicleDockingBay = __instance.GetComponent<VehicleDockingBay>();
                if (!vehicleDockingBay)
                {
                    vehicleDockingBay = __instance.GetComponentInChildren<VehicleDockingBay>();
                }
                Vehicle dealerVehicle = dealer.GetComponent<Vehicle>();
                if (vehicleDockingBay && dealerVehicle)
                {
                    if (vehicleDockingBay.GetDockedVehicle() == dealerVehicle || (Vehicle)vehicleDockingBay.ReflectionGet("interpolatingVehicle") == dealerVehicle
                        || (Vehicle)vehicleDockingBay.ReflectionGet("nearbyVehicle") == dealerVehicle)
                    {
                        Log.Debug($"Dealer {dealer} is vehicle and currently docked or nearby {__instance}, do not harm it!");
                        return false;
                    }
                }
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
                    Optional<NitroxId> dealerId = Optional.Empty;
                    if (dealer)
                    {
                        dealerId = NitroxEntity.GetId(dealer);
                    }
                    NitroxServiceLocator.LocateService<LiveMixinManager>().BroadcastTakeDamage(techType, id, originalDamage, position, type, dealerId, __instance.health);
                }
                ORIGINAL_HEALTH_PER_ENTITY.Remove(id);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
