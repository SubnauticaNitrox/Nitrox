using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CrafterLogic_TryPickup_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrafterLogic t) => t.TryPickup());

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Stfld;
        public static readonly object INJECTION_OPERAND = Reflect.Field((CrafterLogic t) => t.numCrafted);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND, "Operand can not be null");

            bool injected = false;

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND) && !injected)
                {
                    injected = true;

                    /*
                     * Multiplayer.Logic.Crafting.GhostCrafterItemPickedUp(base.gameObject, techType);
                     */
                    yield return TranspilerHelper.LocateService<Crafting>();
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Property((Component t) => t.gameObject).GetMethod);
                    yield return original.Ldloc<TechType>();
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Method((Crafting t) => t.GhostCrafterItemPickedUp(default(GameObject), default(TechType))));
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

