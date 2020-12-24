using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class LiveMixin_TakeDamage_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(LiveMixin);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TakeDamage", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(out float? __state, LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
        {
            __state = null;
            // The result struct is there to reduce calls to simulationOwnership
            ExecutionAndOwnership result = NitroxServiceLocator.LocateService<LiveMixinManager>().ShouldExecute(__instance, -originalDamage, dealer);
            if (result.isOwner)
            {
                // We only fill state with a value if we have the ownership. 
                // This helps us determine if we need to send the change in the postfix
                __state = __instance.health;
            }
            return result.ShouldExecute;
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

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false, false);
        }
    }
}
