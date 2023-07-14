using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Player_SetCurrentSub_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.SetCurrentSub(default(SubRoot), default(bool)));

        public static void Prefix(Player __instance, SubRoot sub)
        {
            // When in the water of the moonpool, it can happen that you hammer change requests
            // while the sub is not changed. This will prevent that
            if (__instance.GetCurrentSub() != sub)
            {
                NitroxServiceLocator.LocateService<LocalPlayer>().BroadcastSubrootChange(sub.GetId());
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
