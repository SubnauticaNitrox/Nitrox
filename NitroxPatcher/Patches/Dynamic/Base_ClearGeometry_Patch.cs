using System.Reflection;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public sealed partial class Base_ClearGeometry_Patch : NitroxPatch, IDynamicPatch
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
    }
}
