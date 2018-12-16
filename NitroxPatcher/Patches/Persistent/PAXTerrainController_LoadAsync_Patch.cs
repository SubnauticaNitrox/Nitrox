using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxModel.Helper;
using NitroxModel.Logger;
using System.Linq;
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
            
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                int nextIndex = i + 1;

                if ((instructionList.Count - 1) > nextIndex)
                {
                    CodeInstruction oneInstructionOut = instructionList[nextIndex];

                   if (oneInstructionOut.opcode.Equals(INJECTION_OPCODE) && oneInstructionOut.operand.Equals(INJECTION_OPERAND))
                   {
                        //Call our injection code and set paxTerratinController.setIsWorking(true); (this keeps up the loading screen)
                        yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Multiplayer).GetMethod("SubnauticaLoadingCompleted", BindingFlags.Public | BindingFlags.Static));
                        yield return new ValidatedCodeInstruction(OpCodes.Ldc_I4_1, instruction.labels);
                   }
                   else
                   {
                        yield return instruction;
                   }
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
            MethodInfo method = getLoadAsyncEnumerableMethod().GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
            Validate.NotNull(method);

            return method;
        }
    }
}

