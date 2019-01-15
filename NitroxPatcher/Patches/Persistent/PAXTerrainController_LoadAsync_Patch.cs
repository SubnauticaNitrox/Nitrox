using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxModel.Helper;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Persistent
{
    public class PAXTerrainController_LoadAsync_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PAXTerrainController);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = TARGET_CLASS.GetMethod("set_isWorking", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            bool alreadyInjected = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (alreadyInjected || instruction.opcode != OpCodes.Ldc_I4_0)
                {
                    yield return instruction;
                }
                else
                {
                    alreadyInjected = true;
                    
                    yield return new CodeInstruction(OpCodes.Call, typeof(Multiplayer).GetMethod(nameof(Multiplayer.SubnauticaLoadingCompleted), BindingFlags.Public | BindingFlags.Static));
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1) { labels = instruction.labels };
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, getMethod());
        }

        private static Type getLoadAsyncEnumerableMethod()
        {
            Type[] nestedTypes = TARGET_CLASS.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static);
            Type targetEnumeratorClass = null;

            foreach (Type type in nestedTypes)
            {
                if (type.FullName.Contains("LoadAsync"))
                {
                    targetEnumeratorClass = type;
                }
            }

            Validate.NotNull(targetEnumeratorClass);
            
            return targetEnumeratorClass;
        }

        private static MethodInfo getMethod()
        {
            MethodInfo method = getLoadAsyncEnumerableMethod().GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
            Validate.NotNull(method);

            return method;
        }
    }
}

