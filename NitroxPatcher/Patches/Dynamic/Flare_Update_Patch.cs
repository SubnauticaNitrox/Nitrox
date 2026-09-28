using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxPatcher.Helper;

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
         * Flare_Update_Patch.UpdateEnergyLeft(this);
         */

        CodeMatcher matcher = new(instructions);

        matcher.MatchEndForward([
                   new CodeMatch(OpCodes.Ldarg_0),
                   new CodeMatch(OpCodes.Ldfld, Reflect.Field((Flare t) => t.energyLeft)),
                   new CodeMatch(OpCodes.Call, Reflect.Property(() => UnityEngine.Time.deltaTime).GetGetMethod()),
                   new CodeMatch(OpCodes.Sub),
                   new CodeMatch(OpCodes.Ldc_R4, 0f),
                   new CodeMatch(OpCodes.Call, Reflect.Method(() => UnityEngine.Mathf.Max(default(float), default(float))))
               ])
               .Advance(-5)
               .RemoveInstructions(6)
               .Insert([
                   new CodeInstruction(OpCodes.Ldarg_0),
                   new CodeInstruction(OpCodes.Call, Reflect.Method(() => UpdateEnergyLeft(default)))
               ]);

        return matcher.InstructionEnumeration();
    }

    public static void UpdateEnergyLeft(Flare flare)
    {
        flare.energyLeft = 1800f - ((float)Resolve<TimeManager>().RealTimeElapsed - flare.flareActivateTime);
    }
}
