using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches
{
    public class Equipment_RemoveItem_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Equipment);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("RemoveItem", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(bool), typeof(bool) }, null);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Stloc_1;

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE))
                {
                    /*
                     * Multiplayer.Logic.EquipmentSlots.Unequip(pickupable, this.owner, slot)
                     */
                    yield return TranspilerHelper.LocateService<EquipmentSlots>();
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(InventoryItem).GetMethod("get_item", BindingFlags.Instance | BindingFlags.Public));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, TARGET_CLASS.GetMethod("get_owner", BindingFlags.Public | BindingFlags.Instance));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(EquipmentSlots).GetMethod("BroadcastUnequip", BindingFlags.Public | BindingFlags.Instance));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
