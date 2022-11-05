using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    class LiveMixin_AddHealth_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((LiveMixin t) => t.AddHealth(default(float)));

        public static bool Prefix(out float? __state, LiveMixin __instance)
        {
            __state = null;

            LiveMixinManager liveMixinManager = Resolve<LiveMixinManager>();
            if (!liveMixinManager.IsWhitelistedUpdateType(__instance))
            {
                return true; // everyone should process this locally
            }

            // Persist the previous health value
            __state = __instance.health;

            return liveMixinManager.ShouldApplyNextHealthUpdate(__instance);
        }

        public static void Postfix(float? __state, LiveMixin __instance, float healthBack)
        {
            // Did we realize a change in health?
            if (__state.HasValue && __state.Value != __instance.health)
            {
                // Let others know if we have a lock on this entity
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                bool hasLock = Resolve<SimulationOwnership>().HasAnyLockType(id);

                if (hasLock)
                {
                    TechType techType = CraftData.GetTechType(__instance.gameObject);
                    Resolve<IPacketSender>().SendIfGameCode<LiveMixinHealthChanged>(new(techType.ToDto(), id, healthBack, __instance.health));
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
