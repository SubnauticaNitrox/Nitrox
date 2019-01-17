using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxModel.Helper;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Persistent
{
    public class EntityCell_AwakeAsync_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EntityCell);

        private static readonly FieldInfo current = getLoadAsyncEnumerableMethod().GetField("$current", BindingFlags.Instance | BindingFlags.NonPublic);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codeInstructions = instructions.ToList();
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldnull);
            yield return new CodeInstruction(OpCodes.Stfld, current);

            int i = codeInstructions.IndexOf(codeInstructions.Last()) - 3;
            Label label = generator.DefineLabel();
            codeInstructions[i].labels.Add(label);
            yield return new CodeInstruction(OpCodes.Br, label);

            foreach (CodeInstruction instruction in codeInstructions)
            {
                yield return instruction;
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
                if (type.FullName.Contains("AwakeAsync"))
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

