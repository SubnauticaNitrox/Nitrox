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
    [HarmonyPatch(typeof(Builder))]
    [HarmonyPatch("TryPlace")]
    public class BuilderPatch
    {
        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = typeof(SkyEnvironmentChanged).GetMethod("Send", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject), typeof(Component) }, null);

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
                     * Multiplayer.PacketSender.BuildItem(Enum.GetName(typeof(TechType), CraftData.GetTechType(Builder.prefab)), Builder.ghostModel.transform.position, Builder.placeRotation);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("PacketSender", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldtoken, typeof(TechType));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(System.Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(RuntimeTypeHandle) }, null));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Builder).GetField("prefab", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(CraftData).GetMethod("GetTechType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject) }, null));
                    yield return new ValidatedCodeInstruction(OpCodes.Box, typeof(TechType));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(System.Enum).GetMethod("GetName"));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Builder).GetField("ghostModel", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(GameObject).GetMethod("get_transform"));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Transform).GetMethod("get_position"));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Builder).GetField("placeRotation", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(PacketSender).GetMethod("BuildItem"));
                }
            }
        }
        
    }
}
