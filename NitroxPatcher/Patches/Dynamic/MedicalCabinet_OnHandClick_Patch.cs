using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class MedicalCabinet_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MedicalCabinet t) => t.OnHandClick(default(GUIHand)));

        public static void Postfix(MedicalCabinet __instance)
        {
            NitroxServiceLocator.LocateService<MedkitFabricator>().Clicked(__instance);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
