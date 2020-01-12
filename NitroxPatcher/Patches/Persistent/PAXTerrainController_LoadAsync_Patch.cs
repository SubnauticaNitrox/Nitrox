using System;
using System.Linq;
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
        public static readonly object INJECTION_OPERAND = typeof(PAXTerrainController).GetMethod("LoadWorldTiles", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator ilGenerator, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);
            List<CodeInstruction> instrList = instructions.ToList();
            Label jmpLabel = ilGenerator.DefineLabel();

            for (int i = 0; i < instrList.Count; i++)
            {
                CodeInstruction instruction = instrList[i];
                if (instrList[i].opcode == OpCodes.Switch)
                {
                    List<Label> labels = ((Label[])instruction.operand).ToList();
                    labels.RemoveRange(3, 5);
                    CodeInstruction codeInstruction = new CodeInstruction(instruction.opcode, labels.ToArray());
                    yield return codeInstruction;
                }
                else if (instruction.opcode == OpCodes.Brtrue && instruction.operand.GetHashCode() == 10)
                {
                    yield return new CodeInstruction(OpCodes.Brtrue, jmpLabel);
                }
                else if (instrList.Count > i + 2 && instrList[i + 1].opcode == OpCodes.Ldfld && instrList[i + 1].operand == (object)typeof(PAXTerrainController).GetField("streamer", BindingFlags.NonPublic | BindingFlags.Instance) && instrList[i+3].opcode == OpCodes.Stfld && instrList[i + 3].operand == (object)typeof(LargeWorldStreamer).GetField("frozen", BindingFlags.Public | BindingFlags.Instance))
                {
                    instruction.labels.Add(jmpLabel);
                    yield return instruction;
                }
                else if (instruction.opcode == OpCodes.Stfld && instruction.operand == (object)typeof(LargeWorldStreamer).GetField("frozen", BindingFlags.Public | BindingFlags.Instance))
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, typeof(Multiplayer).GetMethod(nameof(Multiplayer.SubnauticaLoadingCompleted), BindingFlags.Public | BindingFlags.Static));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_8);
                    yield return new CodeInstruction(OpCodes.Stfld, getStateField(getLoadAsyncEnumerableMethod()));
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Ret);
                }
                else
                {
                    yield return instruction;
                }
            }
        }


        private static FieldInfo getStateField(Type type)
        {
            FieldInfo stateField = null;

            foreach (FieldInfo field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.Name.Contains("state"))
                {
                    stateField = field;
                }
            }
            Validate.NotNull(stateField);
            return stateField;
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

