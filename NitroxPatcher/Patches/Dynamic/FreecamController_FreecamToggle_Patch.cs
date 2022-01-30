using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FreecamController_FreecamToggle_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FreecamController t) => t.FreecamToggle());

        public static void Postfix(FreecamController __instance)
        {
            Resolve<LocalPlayer>().FreecamEnabled = __instance.mode;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
