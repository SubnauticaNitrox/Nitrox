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

        public static bool Prefix(out float? __state, LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
        {
            __state = null;
            // Item1: Should execute; Item2: isSimulationOwner
            // The distinction is there to reduce calls to simulationOwnership
            Tuple<bool, bool> result = NitroxServiceLocator.LocateService<LiveMixinManager>().ShouldExecute(__instance, -originalDamage, dealer);
            if (result.Item2)
            {
                // We only fill state with a value if we have the ownership. 
                // This helps us determine if we need to send the change in the postfix
                __state = __instance.health;
            }
            Log.Debug($"TakeDamagePrefix: should execute: {result}, state: {__state}");
            return result.Item1;
        }

        public static void Postfix(float? __state, LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
        {
            // State is only filled if we have the ownership
            if (__state.HasValue)
            {
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                // Send message to other player if LiveMixin is from a vehicle and got the simulation ownership
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);

                if (__state.Value != __instance.health)
                {
                    Optional<NitroxId> dealerId = Optional.Empty;
                    if (dealer)
                    {
                        dealerId = NitroxEntity.GetId(dealer);
                    }
                    NitroxServiceLocator.LocateService<LiveMixinManager>().BroadcastTakeDamage(techType, id, originalDamage, position, type, dealerId, __instance.health);
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
