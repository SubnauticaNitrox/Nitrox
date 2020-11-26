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
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class LiveMixin_AddHealth_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(LiveMixin);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("AddHealth", BindingFlags.Public | BindingFlags.Instance);
        public static readonly Dictionary<NitroxId, float> ORIGINAL_HEALTH_PER_ENTITY = new Dictionary<NitroxId, float>();
        public static readonly Dictionary<NitroxId, Tuple<LiveMixin, float>> PARAMETER_PER_ENTITY = new Dictionary<NitroxId, Tuple<LiveMixin, float>>();

        public static bool Prefix(LiveMixin __instance, float healthBack)
        {
            Vehicle vehicle = __instance.GetComponent<Vehicle>();
            SubRoot subRoot = __instance.GetComponent<SubRoot>();
            if (vehicle != null || subRoot != null && subRoot.isCyclops)
            {
                SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                if (!simulationOwnership.HasAnyLockType(id) && !simulationOwnership.SimulationLockOverrideActive(id))
                {
                    //Log.Debug($"AddHealth for {__instance.gameObject.name} with heal of {healthBack} detected without lock for {id}. Will not execute the code");
                    // If the player has no knowledge of an other player simulating the vehicle, try to get the simulation ownership
                    if (!simulationOwnership.OtherPlayerHasAnyLock(id))
                    {
                        Log.Debug($"Send simulation lock request in LiveMixin_AddHealth for {id}");
                        simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT, ResponseRecieved);
                        PARAMETER_PER_ENTITY[id] = new Tuple<LiveMixin, float>(__instance, healthBack);
                    }
                    return false;
                }
                //Log.Debug($"AddHealth for {__instance.gameObject.name} with heal of {healthBack} deteted with lock or override for {id}. Will execute code");
                ORIGINAL_HEALTH_PER_ENTITY[id] = __instance.health;
            }
            return true;
        }

        public static void Postfix(LiveMixin __instance, float healthBack)
        {
            Vehicle vehicle = __instance.GetComponent<Vehicle>();
            SubRoot subRoot = __instance.GetComponent<SubRoot>();
            if (vehicle != null || subRoot != null && subRoot.isCyclops)
            {
                GameObject gameObject = __instance.gameObject;
                // Send message to other player if LiveMixin is from a vehicle and got the simulation ownership
                SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
                NitroxId id = NitroxEntity.GetId(gameObject);

                if (simulationOwnership.HasAnyLockType(id) && ORIGINAL_HEALTH_PER_ENTITY[id] != __instance.health)
                {
                    TechType techType = CraftData.GetTechType(gameObject);
                    // This whole code should may be refactored into it's own class if more LiveMixin Entities will be synchronized
                    LiveMixinHealthChanged packet = new LiveMixinHealthChanged(techType.ToDto(), id, healthBack, __instance.health);
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
                Tuple<LiveMixin, float> parameter = null;
                PARAMETER_PER_ENTITY.TryGetValue(id, out parameter);
                if (parameter != null)
                {
                    parameter.Item1.AddHealth(parameter.Item2);
                    PARAMETER_PER_ENTITY.Remove(id);
                }
                else
                {
                    Log.Error($"Got a simulation lock response for entity {id} but did not find the corresponding parameter. This should not happen!");
                }
            }
        }    
    }
}
