using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevent uSkyManager from "freezing" the clouds when FreezeTime is active (game paused).
/// Also sets skybox's rotation depending  on the real server time.
/// </summary>
public class uSkyManager_SetVaryingMaterialProperties_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uSkyManager t) => t.SetVaryingMaterialProperties(default));

    public static readonly InstructionsPattern ModifyInstructionPattern = new()
    {
        Ldarg_0,
        Ldfld,
        { Call, "Modify" },
        Mul
    };

    /// <summary>
    /// Intermediate time property to simplify the dependency resolving for the transpiler.
    /// </summary>
    private static double currentTime
    {
        get
        {
            return Resolve<TimeManager>().CurrentTime;
        }
    }

    /// <summary>
    /// Replaces Time.time call to Time.realtimeSinceStartup so that it doesn't take Time.timeScale into account
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.Transform(ModifyInstructionPattern, (label, instruction) =>
        {
            if (label.Equals("Modify"))
            {
                instruction.operand = Reflect.Property(() => currentTime).GetMethod;
            }
        });

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
