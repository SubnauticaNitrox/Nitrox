using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxModel.Helper;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Persistent
{
    public class PAXTerrainController_LoadAsync_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PAXTerrainController);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = typeof(uGUI_BuilderMenu).GetMethod("EnsureCreated", BindingFlags.Public | BindingFlags.Static);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);
            
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == INJECTION_OPCODE && instruction.operand == INJECTION_OPERAND)
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(Multiplayer).GetMethod(nameof(Multiplayer.SubnauticaLoadingCompleted), BindingFlags.Public | BindingFlags.Static));
                }
                else
                {
                    yield return instruction;
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
            MethodInfo method = getLoadAsyncEnumerableMethod().GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(method);

            return method;
        }
    }
}

