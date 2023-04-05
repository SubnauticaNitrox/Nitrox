using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class RocketPreflightCheckManager_CompletePreflightCheck_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((RocketPreflightCheckManager t) => t.CompletePreflightCheck(default(PreflightCheck)));

        public static void Postfix(RocketPreflightCheckManager __instance)
        {
            Rocket rocket = __instance.gameObject.RequireComponentInParent<Rocket>();

            if (NitroxEntity.TryGetIdOrWarn<RocketPreflightCheckManager_CompletePreflightCheck_Patch>(rocket.gameObject, out NitroxId id))
            {
                Resolve<Entities>().EntityMetadataChanged(rocket, id);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
