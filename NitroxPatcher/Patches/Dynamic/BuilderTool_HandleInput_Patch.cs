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
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((BuilderTool t) => t.HandleInput());

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object INJECTION_OPERAND = Reflect.Method((Constructable t) => t.SetState(default(bool), default(bool)));

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
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Property((Component t) => t.gameObject).GetMethod);
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Method(() => NitroxEntity.GetId(default(GameObject))));
                    yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Method((Building t) => t.DeconstructionBegin(default(NitroxId))));
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
