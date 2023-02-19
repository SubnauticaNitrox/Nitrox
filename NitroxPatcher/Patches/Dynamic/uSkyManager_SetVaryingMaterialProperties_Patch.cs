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
/// Also sets skybox's rotation depending on the real server time.
/// </summary>
public class uSkyManager_SetVaryingMaterialProperties_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((uSkyManager t) => t.SetVaryingMaterialProperties(default));

    /// <summary>
    ///     This pattern detects the <see cref="UnityEngine.Time.time"/> property in the following line
    ///     and replaces the property call target to <see cref="CurrentTime"/>:
    ///     <code>Quaternion q = Quaternion.AngleAxis(cloudsRotateSpeed * Time.time, Vector3.up);</code>
    /// </summary>
    public static readonly InstructionsPattern ModifyInstructionPattern = new()
    {
        Ldarg_0,
        Ldfld,
        { Reflect.Property(() => UnityEngine.Time.time).GetMethod, "Modify" },
        Mul
    };

    /// <summary>
    /// Intermediate time property to simplify the dependency resolving for the transpiler.
    /// </summary>
    private static double CurrentTime => Resolve<TimeManager>().CurrentTime;

    /// <summary>
    /// Replaces Time.time call to Time.realtimeSinceStartup so that it doesn't take Time.timeScale into account
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.Transform(ModifyInstructionPattern, (label, instruction) =>
        {
            if (label.Equals("Modify"))
            {
                instruction.operand = Reflect.Property(() => CurrentTime).GetMethod;
            }
        });

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
