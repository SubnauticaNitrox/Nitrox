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
    public class BuilderPatch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Builder);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TryPlace");

        public static readonly OpCode PLACE_BASE_INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object PLACE_BASE_INJECTION_OPERAND = typeof(BaseGhost).GetMethod("Place");

        public static readonly OpCode PLACE_FURNITURE_INJECTION_OPCODE = OpCodes.Call;
        public static readonly object PLACE_FURNITURE_INJECTION_OPERAND = typeof(SkyEnvironmentChanged).GetMethod("Send", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject), typeof(Component) }, null);
        
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(PLACE_BASE_INJECTION_OPCODE);
            Validate.NotNull(PLACE_BASE_INJECTION_OPERAND);
            Validate.NotNull(PLACE_FURNITURE_INJECTION_OPCODE);
            Validate.NotNull(PLACE_FURNITURE_INJECTION_OPERAND);
            
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(PLACE_BASE_INJECTION_OPCODE) && instruction.operand.Equals(PLACE_BASE_INJECTION_OPERAND))
                {
                    /*
                     * Multiplayer.PacketSender.PlaceBasePiece(componentInParent, CraftData.GetTechType(Builder.prefab), Builder.ghostModel.transform.position, Builder.placeRotation);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("PacketSender", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Builder).GetField("prefab", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(CraftData).GetMethod("GetTechType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject) }, null));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Builder).GetField("ghostModel", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(GameObject).GetMethod("get_transform"));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Transform).GetMethod("get_position"));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Builder).GetField("placeRotation", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(PacketSender).GetMethod("PlaceBasePiece", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(ConstructableBase), typeof(TechType), typeof(Vector3), typeof(Quaternion) }, null));
                }

                if (instruction.opcode.Equals(PLACE_FURNITURE_INJECTION_OPCODE) && instruction.operand.Equals(PLACE_FURNITURE_INJECTION_OPERAND))
                {
                    /*
                     * Multiplayer.PacketSender.PlaceFurniture(gameObject, CraftData.GetTechType(Builder.prefab), Builder.ghostModel.transform.position, Builder.placeRotation);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("PacketSender", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_2);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Builder).GetField("prefab", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(CraftData).GetMethod("GetTechType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject) }, null));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Builder).GetField("ghostModel", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(GameObject).GetMethod("get_transform"));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Transform).GetMethod("get_position"));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Builder).GetField("placeRotation", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(PacketSender).GetMethod("PlaceFurniture", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(GameObject), typeof(TechType), typeof(Vector3), typeof(Quaternion) }, null));
                }                
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
