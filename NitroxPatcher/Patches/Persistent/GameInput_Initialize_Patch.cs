using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public class GameInput_Initialize_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GameInput t) => t.Initialize());

        private static readonly OpCode INJECTION_OPCODE = OpCodes.Stsfld;
        private static readonly object INJECTION_OPERAND = Reflect.Field(() => GameInput.numButtons);

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
                    yield return new CodeInstruction(OpCodes.Newobj, Reflect.Constructor(() => new KeyBindingManager()));
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Method((KeyBindingManager t) => t.GetHighestKeyBindingValue()));
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Add);
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Math.Max(default(int), default(int))));
                }

                yield return instruction;
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
