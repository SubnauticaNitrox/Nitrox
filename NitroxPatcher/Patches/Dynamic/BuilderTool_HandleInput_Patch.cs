using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public class BuilderTool_HandleInput_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((BuilderTool t) => t.HandleInput());

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
    internal static readonly object INJECTION_OPERAND = Reflect.Method((Constructable t) => t.SetState(default(bool), default(bool)));

    private static readonly MethodInfo CALLBACK_METHOD = Reflect.Method((Building t) => t.DeconstructionBegin(default(GameObject)));

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        Validate.NotNull(INJECTION_OPERAND);

        /*
        * constructable.SetState(false, false);
        *
        * to
        *
        * constructable.SetState(false, false);
        *
        * NitroxServiceLocator.LocateService<Building>().DeconstructionBegin(constructable);
        */

        return new CodeMatcher(instructions).MatchStartForward(new CodeMatch(INJECTION_OPCODE, INJECTION_OPERAND))
                                            .MatchStartForward(new CodeMatch(OpCodes.Ret))
                                            .Insert(
                                                TranspilerHelper.LocateService<Building>(),
                                                original.Ldloc<Constructable>(),
                                                new CodeInstruction(OpCodes.Call, CALLBACK_METHOD))
                                            .InstructionEnumeration();
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
