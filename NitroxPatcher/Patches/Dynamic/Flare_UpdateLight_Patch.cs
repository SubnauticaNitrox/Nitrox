using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replaces <see cref="DayNightCycle.main.timePassed"/> with <see cref="TimeManager.RealTimeElapsed"/> in UpdateLight
/// so that light intensity calculations are not affected by time skips
/// </summary>
public sealed partial class Flare_UpdateLight_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Flare t) => t.UpdateLight());

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*
         * REPLACE:
         * float num = (float)(DayNightCycle.main.timePassed - (double)this.flareActivateTime);
         * 
         * WITH:
         * float num = (float)(Resolve<TimeManager>().RealTimeElapsed - (double)this.flareActivateTime);
         */

        CodeMatcher matcher = new(instructions);

        matcher.MatchStartForward([
                   new CodeMatch(OpCodes.Ldsfld, Reflect.Field(() => DayNightCycle.main)),
                   new CodeMatch(OpCodes.Callvirt, Reflect.Property((DayNightCycle t) => t.timePassed).GetGetMethod())
               ])
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, Reflect.Method(() => Resolve<TimeManager>())))
               .SetInstruction(new CodeInstruction(OpCodes.Callvirt, Reflect.Property((TimeManager t) => t.RealTimeElapsed).GetGetMethod()));

        return matcher.InstructionEnumeration();
    }
}
