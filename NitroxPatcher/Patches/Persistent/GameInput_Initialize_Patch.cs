using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public class GameInput_Initialize_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GameInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Stsfld;
        public static readonly object INJECTION_OPERAND = TARGET_CLASS.GetField("numButtons", BindingFlags.Static | BindingFlags.NonPublic);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    /*
                     * int prev = GameInput.GetMaximumEnumValue(typeof(GameInput.Button)) + 1;
                     * //  ^ This value is already calculated by the original code, it's stored on top of the stack.
                     * KeyBindingManager keyBindingManager = new KeyBindingManager();
                     * GameButton.numButtons = Math.Max(keyBindingManager.GetHighestKeyBindingValue() + 1, prev);
                     */
                    yield return new CodeInstruction(OpCodes.Newobj, typeof(KeyBindingManager).GetConstructors().First());
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(KeyBindingManager).GetMethod("GetHighestKeyBindingValue", BindingFlags.Instance | BindingFlags.Public));
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Add);
                    yield return new CodeInstruction(OpCodes.Call, typeof(Math).GetMethod("Max", new[] { typeof(int), typeof(int) }));
                }

                yield return instruction;
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
