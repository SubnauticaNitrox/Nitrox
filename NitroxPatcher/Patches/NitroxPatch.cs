using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches
{
    public abstract class NitroxPatch : INitroxPatch
    {
        private readonly List<MethodBase> activePatches = new List<MethodBase>();

        public abstract void Patch(Harmony harmony);

        public void Restore(Harmony harmony)
        {
            foreach (MethodBase targetMethod in activePatches)
            {
                harmony.Unpatch(targetMethod, HarmonyPatchType.All, harmony.Id);
            }
        }

        public HarmonyMethod GetHarmonyMethod(string methodName)
        {
            MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            Validate.NotNull(method, $"Patcher: Patch method \"{methodName}\" cannot be found");
            return new HarmonyMethod(method);
        }

        protected void PatchTranspiler(Harmony harmony, MethodBase targetMethod, string transpilerMethod = "Transpiler")
        {
            PatchMultiple(harmony, targetMethod, null, null, transpilerMethod);
        }

        protected void PatchPrefix(Harmony harmony, MethodBase targetMethod, string prefixMethod = "Prefix")
        {
            PatchMultiple(harmony, targetMethod, prefixMethod);
        }

        protected void PatchPostfix(Harmony harmony, MethodBase targetMethod, string postfixMethod = "Postfix")
        {
            PatchMultiple(harmony, targetMethod, null, postfixMethod);
        }

        protected void PatchMultiple(Harmony harmony, MethodBase targetMethod, bool prefix, bool postfix, bool transpiler)
        {
            string prefixMethod = prefix ? "Prefix" : null;
            string postfixMethod = postfix ? "Postfix" : null;
            string transpilerMethod = transpiler ? "Transpiler" : null;

            PatchMultiple(harmony, targetMethod, prefixMethod, postfixMethod, transpilerMethod);
        }

        protected void PatchMultiple(Harmony harmony, MethodBase targetMethod,
            string prefixMethod = null, string postfixMethod = null, string transpilerMethod = null)
        {
            Validate.NotNull(targetMethod, "Target method cannot be null");

            HarmonyMethod harmonyPrefixMethod = prefixMethod != null ? GetHarmonyMethod(prefixMethod) : null;
            HarmonyMethod harmonyPostfixMethod = postfixMethod != null ? GetHarmonyMethod(postfixMethod) : null;
            HarmonyMethod harmonyTranspilerMethod = transpilerMethod != null ? GetHarmonyMethod(transpilerMethod) : null;

            harmony.Patch(targetMethod, harmonyPrefixMethod, harmonyPostfixMethod, harmonyTranspilerMethod);
            activePatches.Add(targetMethod); // Store our patched methods
        }
    }
}
