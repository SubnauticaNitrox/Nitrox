using Harmony;
using NitroxModel.Helper;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public abstract class NitroxPatch
    {
        public abstract void Patch(HarmonyInstance harmony);

        public void PatchTranspiler(HarmonyInstance harmony, MethodBase targetMethod)
        {
            Validate.NotNull(targetMethod, "Target method cannot be null");
            harmony.Patch(targetMethod, null, null, GetTranspilerMethod());
        }

        public void PatchPrefix(HarmonyInstance harmony, MethodBase targetMethod)
        {
            Validate.NotNull(targetMethod, "Target method cannot be null");
            harmony.Patch(targetMethod, GetPrefixMethod(), null, null);
        }

        public void PatchPostfix(HarmonyInstance harmony, MethodBase targetMethod)
        {
            Validate.NotNull(targetMethod, "Target method cannot be null");
            harmony.Patch(targetMethod, null, GetPostfixMethod(), null);
        }

        public void PatchMultiple(HarmonyInstance harmony, MethodBase targetMethod, bool prefix, bool postfix, bool transpiler)
        {
            Validate.NotNull(targetMethod, "Target method cannot be null");
            HarmonyMethod prefixMethod = (prefix) ? GetPrefixMethod() : null;
            HarmonyMethod postfixMethod = (postfix) ? GetPostfixMethod() : null;
            HarmonyMethod transpilerMethod = (transpiler) ? GetTranspilerMethod() : null;

            harmony.Patch(targetMethod, prefixMethod, postfixMethod, transpilerMethod);
        }

        public HarmonyMethod GetTranspilerMethod()
        {
            MethodInfo transpiler = this.GetType().GetMethod("Transpiler");
            Validate.NotNull(transpiler, "Transpiler cannot be null");
            return new HarmonyMethod(this.GetType(), "Transpiler");
        }

        public HarmonyMethod GetPostfixMethod()
        {
            MethodInfo postfix = this.GetType().GetMethod("Postfix");
            Validate.NotNull(postfix, "Postfix cannot be null");
            return new HarmonyMethod(this.GetType(), "Postfix");
        }

        public HarmonyMethod GetPrefixMethod()
        {
            MethodInfo prefix = this.GetType().GetMethod("Prefix");
            Validate.NotNull(prefix, "Prefix cannot be null");
            return new HarmonyMethod(this.GetType(), "Prefix");
        }
    }
}
