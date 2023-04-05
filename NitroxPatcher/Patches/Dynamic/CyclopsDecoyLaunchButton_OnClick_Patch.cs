using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    internal class CyclopsDecoyLaunchButton_OnClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsDecoyLaunchButton t) => t.OnClick());

        public static void Postfix(CyclopsHornButton __instance)
        {
            if (NitroxEntity.TryGetIdOrWarn<CyclopsDecoyLaunchButton_OnClick_Patch>(__instance.subRoot.gameObject, out NitroxId id))
            {
                NitroxServiceLocator.LocateService<Cyclops>().BroadcastLaunchDecoy(id);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
