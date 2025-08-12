using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Remove the option for PDA pause
/// </summary>
public sealed partial class uGUI_OptionsPanel_AddAccessibilityTab_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_OptionsPanel t) => t.AddAccessibilityTab());

    /// <summary>
    /// Simply removes following line
    /// AddToggleOption(num, "PDAPause" ...
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward([
                                                new(OpCodes.Ldarg_0),
                                                new(OpCodes.Ldloc_0),
                                                new(OpCodes.Ldstr, "PDAPause")
                                            ])
                                            .RemoveInstructions(10)
                                            .InstructionEnumeration();
    }
}
