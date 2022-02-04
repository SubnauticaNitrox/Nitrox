using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Equipment_RemoveItem_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Equipment t) => t.RemoveItem(default(string), default(bool), default(bool)));

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
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Property((InventoryItem t) => t.item).GetMethod);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Property((Equipment t) => t.owner).GetMethod);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Method((EquipmentSlots t) => t.BroadcastUnequip(default(Pickupable), default(GameObject), default(string))));
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
