using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BuilderTool_HandleInput_Patch : NitroxPatch, IDynamicPatch
    {
        internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((BuilderTool t) => t.HandleInput());

        internal static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
        internal static readonly object INJECTION_OPERAND = Reflect.Method((Constructable t) => t.SetState(default(bool), default(bool)));
        private static readonly MethodInfo COMPONENT_GAMEOBJECT_GETTER = Reflect.Property((Component t) => t.gameObject).GetMethod;
        private static readonly MethodInfo NITROXENTITY_GETID = Reflect.Method(() => NitroxEntity.GetId(default(GameObject)));
        private static readonly MethodInfo BUILDING_DESCONSTRUCTIONBEGIN = Reflect.Method((Building t) => t.DeconstructionBegin(default(NitroxId)));

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    /*
                     * Multiplayer.Logic.Building.DeconstructionBegin(constructable.gameObject);
                     */
                    yield return TranspilerHelper.LocateService<Building>();
                    yield return original.Ldloc<Constructable>();
                    yield return new CodeInstruction(OpCodes.Callvirt, COMPONENT_GAMEOBJECT_GETTER);
                    yield return new CodeInstruction(OpCodes.Callvirt, NITROXENTITY_GETID);
                    yield return new CodeInstruction(OpCodes.Callvirt, BUILDING_DESCONSTRUCTIONBEGIN);
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
