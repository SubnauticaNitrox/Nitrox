using Harmony;
using Harmony.ILCopying;
using NitroxClient.Communication;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Constructable_Construct_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Construct");

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = typeof(Constructable).GetMethod("UpdateMaterial", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPCODE);
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    /*
                     * Multiplayer.PacketSender.ChangeConstructionAmount(base.gameObject.transform.position, this.constructedAmount, resourceID, resourceID2);
                     */                     
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("PacketSender", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Component).GetMethod("get_gameObject", BindingFlags.Public | BindingFlags.Instance));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(GameObject).GetMethod("get_transform"));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Transform).GetMethod("get_position"));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldfld, typeof(Constructable).GetField("constructedAmount", BindingFlags.Public | BindingFlags.Instance));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_1);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_2);
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(PacketSender).GetMethod("ChangeConstructionAmount"));              
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
