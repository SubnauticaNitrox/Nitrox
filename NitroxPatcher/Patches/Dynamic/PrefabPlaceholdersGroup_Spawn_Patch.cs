using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PrefabPlaceholdersGroup_Spawn_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrefabPlaceholdersGroup t) => t.Spawn(default(bool), default(bool)));

        public static bool Prefix()
        {
            return false; // Disable spawning of PrefabPlaceholders(In other words large portion of objects)
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
