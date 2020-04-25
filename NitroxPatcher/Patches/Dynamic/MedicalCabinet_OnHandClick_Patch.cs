using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class MedicalCabinet_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(MedicalCabinet).GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(MedicalCabinet __instance)
        {
            NitroxServiceLocator.LocateService<MedkitFabricator>().Clicked(__instance);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
