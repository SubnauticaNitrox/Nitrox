using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class LiveMixin_TakeDamage_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((LiveMixin t) => t.TakeDamage(default(float), default(Vector3), default(DamageType), default(GameObject)));

        public static bool Prefix(out float? __state, LiveMixin __instance, GameObject dealer)
        {
            __state = null;

            LiveMixinManager liveMixinManager = NitroxServiceLocator.LocateService<LiveMixinManager>();

            if (!liveMixinManager.IsWhitelistedUpdateType(__instance))
            {
                return true; // everyone should process this locally
            }

            // Persist the previous health value
            __state = __instance.health;

            return liveMixinManager.ShouldApplyNextHealthUpdate(__instance, dealer);
        }

        public static void Postfix(float? __state, LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
        {
            // Did we realize a change in health?
            if (__state.HasValue && __state.Value != __instance.health)
            {
                // Let others know if we have a lock on this entity
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                bool hasLock = NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(id);

                if (hasLock)
                {
                    TechType techType = CraftData.GetTechType(__instance.gameObject);
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
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
