using Harmony;
using NitroxClient.Communication;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Pickupable_Drop_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Pickupable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Drop", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) }, null);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = typeof(FMODUWE).GetMethod("PlayOneShot", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(Vector3), typeof(float) }, null);

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
                     * Multiplayer.PacketSender.DropItem(base.gameObject, this.GetTechType(), dropPosition);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("PacketSender", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Component).GetMethod("get_gameObject", BindingFlags.Public | BindingFlags.Instance));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Pickupable).GetMethod("GetTechType", BindingFlags.Public | BindingFlags.Instance));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_1);
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(PacketSender).GetMethod("DropItem"));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

