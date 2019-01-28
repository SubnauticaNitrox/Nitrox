using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Builder_TryPlace_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Builder);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TryPlace");

        public static readonly OpCode PLACE_BASE_INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object PLACE_BASE_INJECTION_OPERAND = typeof(BaseGhost).GetMethod("Place");

        public static readonly OpCode PLACE_FURNITURE_INJECTION_OPCODE = OpCodes.Call;
        public static readonly object PLACE_FURNITURE_INJECTION_OPERAND = typeof(SkyEnvironmentChanged).GetMethod("Send", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject), typeof(Component) }, null);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(PLACE_BASE_INJECTION_OPERAND);
            Validate.NotNull(PLACE_FURNITURE_INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(PLACE_BASE_INJECTION_OPCODE) && instruction.operand.Equals(PLACE_BASE_INJECTION_OPERAND))
                {
                    /*
                     *  Multiplayer.Logic.Building.PlaceBasePiece(componentInParent, component.TargetBase, CraftData.GetTechType(Builder.prefab), Builder.placeRotation);
                     */
                    yield return TranspilerHelper.LocateService<Building>();
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(BaseGhost).GetMethod("get_TargetBase"));
                    yield return new CodeInstruction(OpCodes.Ldsfld, TARGET_CLASS.GetField("prefab", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new CodeInstruction(OpCodes.Call, typeof(CraftData).GetMethod("GetTechType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject) }, null));
                    yield return new CodeInstruction(OpCodes.Ldsfld, TARGET_CLASS.GetField("placeRotation", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(Building).GetMethod("PlaceBasePiece", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(BaseGhost), typeof(ConstructableBase), typeof(Base), typeof(TechType), typeof(Quaternion) }, null));
                }

                if (instruction.opcode.Equals(PLACE_FURNITURE_INJECTION_OPCODE) && instruction.operand.Equals(PLACE_FURNITURE_INJECTION_OPERAND))
                {
                    /*
                     *  Multiplayer.Logic.Building.PlaceFurniture(gameObject, CraftData.GetTechType(Builder.prefab), Builder.ghostModel.transform.position, Builder.placeRotation);
                     */
                    yield return TranspilerHelper.LocateService<Building>();
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldsfld, TARGET_CLASS.GetField("prefab", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new CodeInstruction(OpCodes.Call, typeof(CraftData).GetMethod("GetTechType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(GameObject) }, null));
                    yield return new CodeInstruction(OpCodes.Ldsfld, TARGET_CLASS.GetField("ghostModel", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(GameObject).GetMethod("get_transform"));
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(Transform).GetMethod("get_position"));
                    yield return new CodeInstruction(OpCodes.Ldsfld, TARGET_CLASS.GetField("placeRotation", BindingFlags.Static | BindingFlags.NonPublic));
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(Building).GetMethod("PlaceFurniture", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(GameObject), typeof(TechType), typeof(Vector3), typeof(Quaternion) }, null));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
