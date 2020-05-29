using Harmony;
using System.Reflection;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    class Base_CopyFrom_Patch : NitroxPatch, IDynamicPatch
    {
        public readonly MethodInfo METHOD = typeof(Base).GetMethod(nameof(Base.CopyFrom), BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(Base __instance, Base sourceBase)
        {
            NitroxServiceLocator.LocateService<Building>().Base_CopyFrom_Pre(__instance, sourceBase);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, METHOD);
        }
    }
}
