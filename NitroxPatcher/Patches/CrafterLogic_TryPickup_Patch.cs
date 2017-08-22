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
    public class CrafterLogic_TryPickup_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CrafterLogic);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TryPickup", BindingFlags.Public | BindingFlags.Instance);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Stfld;
        public static readonly object INJECTION_OPERAND = TARGET_CLASS.GetField("numCrafted", BindingFlags.Public | BindingFlags.Instance);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPCODE, "Opcode can not be null");
            Validate.NotNull(INJECTION_OPERAND, "Operand can not be null");

            bool injected = false;

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND) && !injected)
                {
                    injected = true;

                    /*
                     * Multiplayer.Logic.Crafting.FabricatorItemPickedUp(base.gameObject, techType);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("Logic", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Logic).GetMethod("get_Crafting", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Component).GetMethod("get_gameObject", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_S, GetLocalVariableIndex<TechType>(original));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Crafting).GetMethod("FabricatorItemPickedUp", BindingFlags.Instance | BindingFlags.Public));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

