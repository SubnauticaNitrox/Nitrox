using Harmony;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NitroxPatcher.Patches
{
    public abstract class NitroxPatch
    {
        public abstract void Patch(HarmonyInstance harmony);

        public void PatchTranspiler(HarmonyInstance harmony, MethodBase targetMethod)
        {
            Validate.NotNull(targetMethod, "Target method cannot be null");
            MethodInfo transpiler = this.GetType().GetMethod("Transpiler");
            Validate.NotNull(transpiler, "Transpiler cannot be null");
            HarmonyMethod harmonyMethod = new HarmonyMethod(this.GetType(), "Transpiler");
            harmony.Patch(targetMethod, null, null, harmonyMethod);
        }

        public void PatchPrefix(HarmonyInstance harmony, MethodBase targetMethod)
        {
            Validate.NotNull(targetMethod, "Target method cannot be null");
            MethodInfo prefix = this.GetType().GetMethod("Prefix");
            Validate.NotNull(prefix, "Prefix cannot be null");
            HarmonyMethod harmonyMethod = new HarmonyMethod(this.GetType(), "Prefix");
            harmony.Patch(targetMethod, harmonyMethod, null, null);
        }

        public void PatchPostfix(HarmonyInstance harmony, MethodBase targetMethod)
        {
            Validate.NotNull(targetMethod, "Target method cannot be null");
            MethodInfo postfix = this.GetType().GetMethod("Postfix");
            Validate.NotNull(postfix, "Postfix cannot be null");
            HarmonyMethod harmonyMethod = new HarmonyMethod(this.GetType(), "Postfix");
            harmony.Patch(targetMethod, null, harmonyMethod, null);
        }
    }
}
