using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches
{
    public abstract class NitroxPatch
    {
        private readonly List<PatchProcessor> activePatches = new List<PatchProcessor>();

        public abstract void Patch(HarmonyInstance harmony);

        protected void PatchTranspiler(HarmonyInstance harmony, MethodBase targetMethod, string transpilerMethod = "Transpiler")
        {
            PatchMultiple(harmony, targetMethod, null, null, transpilerMethod);
        }

        protected void PatchPrefix(HarmonyInstance harmony, MethodBase targetMethod, string prefixMethod = "Prefix")
        {
            PatchMultiple(harmony, targetMethod, prefixMethod, null, null);
        }

        protected void PatchPostfix(HarmonyInstance harmony, MethodBase targetMethod, string postfixMethod = "Postfix")
        {
            PatchMultiple(harmony, targetMethod, null, postfixMethod, null);
        }

        protected void PatchMultiple(HarmonyInstance harmony, MethodBase targetMethod, bool prefix, bool postfix, bool transpiler)
        {
            string prefixMethod = (prefix) ? "Prefix" : null;
            string postfixMethod = (postfix) ? "Postfix" : null;
            string transpilerMethod = (transpiler) ? "Transpiler" : null;

            PatchMultiple(harmony, targetMethod, prefixMethod, postfixMethod, transpilerMethod);
        }

        protected void PatchMultiple(HarmonyInstance harmony, MethodBase targetMethod,
            string prefixMethod = null, string postfixMethod = null, string transpilerMethod = null)
        {
            Validate.NotNull(targetMethod, "Target method cannot be null");

            HarmonyMethod harmonyPrefixMethod = (prefixMethod != null) ? GetHarmonyMethod(prefixMethod) : null;
            HarmonyMethod harmonyPostfixMethod = (postfixMethod != null) ? GetHarmonyMethod(postfixMethod) : null;
            HarmonyMethod harmonyTranspilerMethod = (transpilerMethod != null) ? GetHarmonyMethod(transpilerMethod) : null;

            // TODO: Cache these objects anyway to prevent recreating them every time the patches are applied.
            activePatches.Add(
                harmony.Patch(targetMethod, harmonyPrefixMethod, harmonyPostfixMethod, harmonyTranspilerMethod)
            );
        }

        public void Restore()
        {
            foreach (PatchProcessor patch in activePatches)
            {
                patch.Restore();
            }

            activePatches.Clear();
        }

        public HarmonyMethod GetHarmonyMethod(string methodName)
        {
            MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            Validate.NotNull(method, $"Patcher: Patch method \"{methodName}\" cannot be found");
            return new HarmonyMethod(method);
        }
    }
}
