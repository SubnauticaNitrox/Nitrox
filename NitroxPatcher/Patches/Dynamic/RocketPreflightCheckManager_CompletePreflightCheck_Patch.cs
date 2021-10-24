using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class RocketPreflightCheckManager_CompletePreflightCheck_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((RocketPreflightCheckManager t) => t.CompletePreflightCheck(default(PreflightCheck)));

        public static void Prefix(RocketPreflightCheckManager __instance, PreflightCheck completeCheck)
        {
            Rocket rocket = __instance.gameObject.RequireComponentInParent<Rocket>();
            NitroxId id = NitroxEntity.GetId(rocket.gameObject);

            NitroxServiceLocator.LocateService<Rockets>().CompletePreflightCheck(id, completeCheck);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
