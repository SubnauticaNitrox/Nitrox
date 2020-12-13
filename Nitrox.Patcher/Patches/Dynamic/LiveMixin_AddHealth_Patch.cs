using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;

namespace Nitrox.Patcher.Patches.Dynamic
{
    class LiveMixin_AddHealth_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(LiveMixin);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("AddHealth", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(out float? __state, LiveMixin __instance, float healthBack)
        {
            __state = null;
            // The result struct is there to reduce calls to simulationOwnership
            ExecutionAndOwnership result = NitroxServiceLocator.LocateService<LiveMixinManager>().ShouldExecute(__instance, healthBack, null);
            if (result.isOwner)
            {
                // We only fill state with a value if we have the ownership. 
                // This helps us determine if we need to send the change in the postfix
                __state = __instance.health;
            }
            return result.ShouldExecute;
        }

        public static void Postfix(float? __state, LiveMixin __instance, float healthBack)
        {
            // State is only filled if we have the ownership
            if (__state.HasValue)
            {
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                // Send message to other player if LiveMixin is from a vehicle and got the simulation ownership
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);

                if (__state.Value != __instance.health)
                {
                    NitroxServiceLocator.LocateService<LiveMixinManager>().BroadcastAddHealth(techType, id, healthBack, __instance.health);
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }   
    }
}
