using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class BuilderTool_HandleInput_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BuilderTool);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("HandleInput", BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object INJECTION_OPERAND = typeof(Constructable).GetMethod("SetState", BindingFlags.Public | BindingFlags.Instance);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    /*
                     * Multiplayer.Logic.Building.DeconstructionBegin(constructable.gameObject);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("Logic", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Logic).GetMethod("get_Building", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_S, GetLocalVariableIndex<Constructable>(original));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Component).GetMethod("get_gameObject", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Building).GetMethod("DeconstructionBegin", BindingFlags.Public | BindingFlags.Instance));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
