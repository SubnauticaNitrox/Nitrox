using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;

namespace Nitrox.Patcher.Patches.Dynamic
{
    public class RocketPreflightCheckManager_CompletePreflightCheck_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(RocketPreflightCheckManager).GetMethod("CompletePreflightCheck", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(RocketPreflightCheckManager __instance, PreflightCheck completeCheck)
        {
            Rocket rocket = __instance.gameObject.RequireComponentInParent<Rocket>();
            NitroxId id = NitroxEntity.GetId(rocket.gameObject);

            NitroxServiceLocator.LocateService<Rockets>().CompletePreflightCheck(id, completeCheck);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
