using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class Inventory_LoseItems_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((Inventory t) => t.LoseItems());

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
#if SUBNAUTICA
    internal static readonly object INJECTION_OPERAND = Reflect.Method((Inventory t) => t.InternalDropItem(default(Pickupable), default(bool)));
#elif BELOWZERO
    internal static readonly object INJECTION_OPERAND = Reflect.Method((Inventory t) => t.InternalDropItem(default(Pickupable), default(bool), default(bool)));
#endif

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codeInstructions = new(instructions);
        for (int i = 0; i < codeInstructions.Count; i++)
        {
            CodeInstruction instruction = codeInstructions[i];
#if SUBNAUTICA
            /* if (this.InternalDropItem(list[i].item, false))
             * becomes:
             * if (Inventory_LoseItems_Patch.DropAndDeleteItem(list[i].item))
             */

            // Clear some useless lines
            if (instruction.opcode.Equals(OpCodes.Ldarg_0) &&
                codeInstructions[i + 1].opcode.Equals(OpCodes.Ldloc_0) &&
                codeInstructions[i - 1].opcode.Equals(OpCodes.Br))
            {
                // We still need to transfer these labels to the following instruction
                codeInstructions[i + 1].labels.AddRange(instruction.labels);
                continue;
            }
            if (instruction.opcode.Equals(OpCodes.Ldc_I4_0) &&
                codeInstructions[i + 1].opcode.Equals(OpCodes.Call) &&
                codeInstructions[i - 1].opcode.Equals(OpCodes.Callvirt))
            {
                continue;
            }
#elif BELOWZERO
            /* if (this.InternalDropItem(inventoryItem.item, false, true))
             * becomes:
             * if (Inventory_LoseItems_Patch.DropAndDeleteItem(inventoryItem.item))
             */

            // Clear some useless lines
            if (instruction.opcode.Equals(OpCodes.Ldarg_0) &&
                codeInstructions[i + 1].opcode.Equals(OpCodes.Ldloc_3) &&
                codeInstructions[i - 1].opcode.Equals(OpCodes.Bne_Un_S))
            {
                // We still need to transfer these labels to the following instruction
                codeInstructions[i + 1].labels.AddRange(instruction.labels);
                continue;
            }
            if (instruction.opcode.Equals(OpCodes.Ldc_I4_1) &&
                codeInstructions[i + 1].opcode.Equals(OpCodes.Call) &&
                codeInstructions[i - 2].opcode.Equals(OpCodes.Callvirt))
            {
                continue;
            }

            // Clear some useless lines
            if (instruction.opcode.Equals(OpCodes.Ldarg_0) &&
                codeInstructions[i + 1].opcode.Equals(OpCodes.Ldloc_0) &&
                codeInstructions[i - 1].opcode.Equals(OpCodes.Br))
            {
                // We still need to transfer these labels to the following instruction
                codeInstructions[i + 1].labels.AddRange(instruction.labels);
                continue;
            }
            if (instruction.opcode.Equals(OpCodes.Ldc_I4_0) &&
                codeInstructions[i + 2].opcode.Equals(OpCodes.Call) &&
                codeInstructions[i - 1].opcode.Equals(OpCodes.Callvirt))
            {
                continue;
            }
#endif

            // And modify the call instruction
            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                instruction.operand = Reflect.Method(() => DropAndDeleteItem(default));
            }
            yield return instruction;
        }
    }

    public static bool DropAndDeleteItem(Pickupable pickupable)
    {
        if (Inventory.CanDropItemHere(pickupable, false))
        {
            // Here limit the part that spreads the stuff around by deactivating the rigidbody (for when we drop them)
            pickupable.Drop(pickupable.transform.position, default, false);
            // And we destroy the item to make sure that it won't stay after the zone unloads
            UnityEngine.Object.Destroy(pickupable.gameObject);
            return true;
        }
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
