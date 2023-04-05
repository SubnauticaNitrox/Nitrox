using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public class BuilderTool_HandleInput_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((BuilderTool t) => t.HandleInput());

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
    internal static readonly object INJECTION_OPERAND = Reflect.Method((Constructable t) => t.SetState(default(bool), default(bool)));
    private static readonly MethodInfo COMPONENT_GAMEOBJECT_GETTER = Reflect.Property((Component t) => t.gameObject).GetMethod;
    private static readonly MethodInfo BUILDING_DESCONSTRUCTIONBEGIN = Reflect.Method((Building t) => t.DeconstructionBegin(default(NitroxId)));

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        Validate.NotNull(INJECTION_OPERAND);

        LocalBuilder idBuilder = generator.DeclareLocal(typeof(NitroxId));

        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;
            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                /*
                 * constructable.SetState(false, false);
                 *
                 * to
                 *
                 * constructable.SetState(false, false);
                 *
                 * if (NitroxEntity.TryGetIdFrom(constructable.gameObject, out NitroxId id))
                 * {
                 *    Multiplayer.Logic.Building.DeconstructionBegin(id);
                 * }
                 * else
                 * {
                 *     Log.Error(message);
                 * }
                 */

                foreach (CodeInstruction helperInstruction in
                         TranspilerHelper.TryGetIdOrLogError(generator, idBuilder, "[BuilderTool_HandleInput_Patch] Couldn't get id",
                                                             new[]
                                                             {
                                                                 original.Ldloc<Constructable>(),
                                                                 new CodeInstruction(OpCodes.Callvirt, COMPONENT_GAMEOBJECT_GETTER)
                                                             },
                                                             new[]
                                                             {
                                                                 TranspilerHelper.LocateService<Building>(),
                                                                 TranspilerHelper.Ldloc(idBuilder),
                                                                 new CodeInstruction(OpCodes.Callvirt, BUILDING_DESCONSTRUCTIONBEGIN)
                                                             }))
                {
                    yield return helperInstruction;
                }
            }
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
