using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public class PAXTerrainController_LoadAsync_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly Type TARGET_CLASS = typeof(PAXTerrainController);
        private static readonly object INJECTION_OPERAND = Reflect.Method((PAXTerrainController t) => t.LoadWorldTiles());
        private static readonly FieldInfo LARGE_WORLD_STREAMER_FROZEN_FIELD = Reflect.Field((LargeWorldStreamer t) => t.frozen);
        private static readonly FieldInfo PAX_TERRAIN_CONTROLLER_STREAMER_FIELD = Reflect.Field((PAXTerrainController t) => t.streamer);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator ilGenerator, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);
            List<CodeInstruction> instrList = instructions.ToList();
            Label jmpLabelStartOfMethod = ilGenerator.DefineLabel();

            for (int i = 0; i < instrList.Count; i++)
            {
                CodeInstruction instruction = instrList[i];
                if (instrList[i].opcode == OpCodes.Switch)
                {
                    List<Label> labels = ((Label[])instruction.operand).ToList(); // removing unneccessary labels
                    labels.RemoveRange(3, 5);
                    yield return new CodeInstruction(instruction.opcode, labels.ToArray());
                }
                else if (instruction.opcode == OpCodes.Brtrue || instruction.opcode == OpCodes.Brtrue_S)
                {
                    yield return new CodeInstruction(OpCodes.Brtrue, jmpLabelStartOfMethod); // replace previous jump with new one
                }
                else if (instrList.Count > i + 2 &&
                         instrList[i + 1].opcode == OpCodes.Ldfld &&
                         Equals(instrList[i + 1].operand, PAX_TERRAIN_CONTROLLER_STREAMER_FIELD) &&
                         instrList[i + 3].opcode == OpCodes.Stfld &&
                         Equals(instrList[i + 3].operand, LARGE_WORLD_STREAMER_FROZEN_FIELD))
                {
                    instruction.labels.Add(jmpLabelStartOfMethod);
                    yield return instruction; // Add a label for jumping
                }
                else if (instruction.opcode == OpCodes.Stfld &&
                         Equals(instruction.operand, LARGE_WORLD_STREAMER_FROZEN_FIELD))
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Multiplayer.SubnauticaLoadingCompleted()));
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                    yield return new CodeInstruction(OpCodes.Ldc_I4_8); // Load 8 onto the stack
                    yield return new CodeInstruction(OpCodes.Stfld, GetStateField(GetLoadAsyncEnumerableMethod())); // Store last stack item into state field
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1); // return true
                    yield return new CodeInstruction(OpCodes.Ret);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, GetMethod());
        }

        private static FieldInfo GetStateField(IReflect type)
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

        private static Type GetLoadAsyncEnumerableMethod()
        {
            Type[] nestedTypes = TARGET_CLASS.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static);
            Type targetEnumeratorClass = null;

            foreach (Type type in nestedTypes)
            {
                if (type.FullName?.Contains("LoadAsync") == true)
                {
                    targetEnumeratorClass = type;
                }
            }

            Validate.NotNull(targetEnumeratorClass);
            return targetEnumeratorClass;
        }

        private static MethodInfo GetMethod()
        {
            MethodInfo method = GetLoadAsyncEnumerableMethod().GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(method);

            return method;
        }
    }
}
