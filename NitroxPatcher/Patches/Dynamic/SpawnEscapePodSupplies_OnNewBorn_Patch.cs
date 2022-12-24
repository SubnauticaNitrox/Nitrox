using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using UWE;

/// <summary>
/// Don't allow the game to spawn initial supplies in the escape pod.
/// </summary>
namespace NitroxPatcher.Patches.Dynamic
{
    public class SpawnEscapePodSupplies_OnNewBorn_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpawnEscapePodSupplies t) => t.OnNewBorn());

        public static bool Prefix()
        {
            return false;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
