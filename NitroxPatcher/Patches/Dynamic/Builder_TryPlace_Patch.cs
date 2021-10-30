using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Builder_TryPlace_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => Builder.TryPlace());

        public static readonly OpCode PLACE_BASE_INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object PLACE_BASE_INJECTION_OPERAND = Reflect.Method((BaseGhost t) => t.Place());

        public static readonly OpCode PLACE_FURNITURE_INJECTION_OPCODE = OpCodes.Call;
        public static readonly object PLACE_FURNITURE_INJECTION_OPERAND = Reflect.Method(() => SkyEnvironmentChanged.Send(default(GameObject), default(Component)));

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
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Property((BaseGhost t) => t.TargetBase).GetMethod);
                    yield return new CodeInstruction(OpCodes.Ldsfld, Reflect.Field(() => Builder.prefab));
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => CraftData.GetTechType(default(GameObject))));
                    yield return new CodeInstruction(OpCodes.Ldsfld, Reflect.Field(() => Builder.placeRotation));
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Method((Building t) => t.PlaceBasePiece(default(BaseGhost), default(ConstructableBase), default(Base), default(TechType), default(Quaternion))));
                }

                if (instruction.opcode.Equals(PLACE_FURNITURE_INJECTION_OPCODE) && instruction.operand.Equals(PLACE_FURNITURE_INJECTION_OPERAND))
                {
                    /*
                     *  Multiplayer.Logic.Building.PlaceFurniture(gameObject, CraftData.GetTechType(Builder.prefab), Builder.ghostModel.transform.position, Builder.placeRotation);
                     */
                    yield return TranspilerHelper.LocateService<Building>();
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldsfld, Reflect.Field(() => Builder.prefab));
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => CraftData.GetTechType(default(GameObject))));
                    yield return new CodeInstruction(OpCodes.Ldsfld, Reflect.Field(() => Builder.ghostModel));
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Property((GameObject t) => t.transform).GetMethod);
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Property((Transform t) => t.position).GetMethod);
                    yield return new CodeInstruction(OpCodes.Ldsfld, Reflect.Field(() => Builder.placeRotation));
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Method((Building t) => t.PlaceFurniture(default(GameObject), default(TechType), default(Vector3), default(Quaternion))));
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
