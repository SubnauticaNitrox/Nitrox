using Harmony;
using NitroxClient.Communication;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Constructable_Construct_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Construct");

        public static readonly OpCode AMOUNT_CHANGED_INJECTION_OPCODE = OpCodes.Ldsfld;
        public static readonly object AMOUNT_CHANGED_INJECTION_OPERAND = typeof(Inventory).GetField("main", BindingFlags.Public | BindingFlags.Static);

        public static readonly OpCode CONSTRUCTION_COMPLETE_INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object CONSTRUCTION_COMPLETE_INJECTION_OPERAND = typeof(Constructable).GetMethod("SetState", BindingFlags.Public | BindingFlags.Instance);
        
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(AMOUNT_CHANGED_INJECTION_OPCODE);
            Validate.NotNull(AMOUNT_CHANGED_INJECTION_OPERAND);
            Validate.NotNull(CONSTRUCTION_COMPLETE_INJECTION_OPCODE);
            Validate.NotNull(CONSTRUCTION_COMPLETE_INJECTION_OPERAND);
            
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode.Equals(AMOUNT_CHANGED_INJECTION_OPCODE) && instruction.operand.Equals(AMOUNT_CHANGED_INJECTION_OPERAND))
                {
                    /*
                     * Multiplayer.PacketSender.ChangeConstructionAmount(base.gameObject.transform.position, this.constructedAmount);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("PacketSender", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Component).GetMethod("get_gameObject", BindingFlags.Public | BindingFlags.Instance));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldfld, typeof(Constructable).GetField("constructedAmount", BindingFlags.Public | BindingFlags.Instance));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(PacketSender).GetMethod("ChangeConstructionAmount", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(GameObject), typeof(float) }, null));
                }

                yield return instruction;

                if (instruction.opcode.Equals(CONSTRUCTION_COMPLETE_INJECTION_OPCODE) && instruction.operand.Equals(CONSTRUCTION_COMPLETE_INJECTION_OPERAND))
                {
                    /*
                     * Multiplayer.PacketSender.ConstructionComplete(base.gameObject);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("PacketSender", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Component).GetMethod("get_gameObject", BindingFlags.Public | BindingFlags.Instance));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(PacketSender).GetMethod("ConstructionComplete"));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
