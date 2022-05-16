using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Base_ClearGeometry_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Base t) => t.ClearGeometry());

        public static void Prefix(Base __instance)
        {
            if (__instance == null)
            {
                return;
            }
            
            Resolve<GeometryRespawnManager>().GeometryClearedForBase(__instance);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
