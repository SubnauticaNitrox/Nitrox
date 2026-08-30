using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxPatcher.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replaces energy consumption calculation to use <see cref="TimeManager.RealTimeElapsed"/>
/// instead of <see cref="Time.deltaTime"/> to make energy drain immune to time skips
/// </summary>
public sealed partial class Flare_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Flare t) => t.Update());

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*
         * REPLACE:
         * this.energyLeft = Mathf.Max(this.energyLeft - Time.deltaTime, 0f);
         * 
         * WITH:
         * this.energyLeft = 1800f - ((float)Resolve<TimeManager>().RealTimeElapsed - this.flareActivateTime);
         */

        CodeMatcher matcher = new(instructions);

        matcher.MatchEndForward([
                   new CodeMatch(OpCodes.Ldarg_0),
                   new CodeMatch(OpCodes.Ldfld, Reflect.Field((Flare t) => t.energyLeft)),
                   new CodeMatch(OpCodes.Call, Reflect.Property(() => Time.deltaTime).GetGetMethod()),
                   new CodeMatch(OpCodes.Sub),
                   new CodeMatch(OpCodes.Ldc_R4, 0f),
                   new CodeMatch(OpCodes.Call, Reflect.Method(() => UnityEngine.Mathf.Max(default(float), default(float))))
               ])
               .Advance(-5)
               .RemoveInstructions(6)
               .Insert([
                   new CodeInstruction(OpCodes.Ldc_R4, 1800f), // Max energy
                   new CodeInstruction(OpCodes.Call, Reflect.Method(() => Resolve<TimeManager>())),
                   new CodeInstruction(OpCodes.Callvirt, Reflect.Property((TimeManager t) => t.RealTimeElapsed).GetGetMethod()),
                   new CodeInstruction(OpCodes.Conv_R4),
                   new CodeInstruction(OpCodes.Ldarg_0),
                   new CodeInstruction(OpCodes.Ldfld, Reflect.Field((Flare t) => t.flareActivateTime)),
                   new CodeInstruction(OpCodes.Sub),
                   new CodeInstruction(OpCodes.Sub)
               ]);

        return matcher.InstructionEnumeration();
    }
}
