using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Radio_OnRepair_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(Radio).GetMethod("OnRepair", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(Radio __instance)
        {
            NitroxServiceLocator.LocateService<EscapePodManager>().OnRadioRepairedByMe(__instance);
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
