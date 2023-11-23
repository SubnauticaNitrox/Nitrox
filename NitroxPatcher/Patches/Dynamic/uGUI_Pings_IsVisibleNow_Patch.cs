using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Decide whether or not we want to render the pings on the screen
/// </summary>
public sealed partial class uGUI_Pings_IsVisibleNow_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TargetMethod = Reflect.Method((uGUI_Pings t) => t.IsVisibleNow());

    private static readonly object injectionCall = Reflect.Method(() => IsVisible());

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        // Instead of each return false (Ldc_I4_0) we call our injection method and return then.
        return new CodeMatcher(instructions).MatchStartForward(
                                                new CodeMatch(OpCodes.Ldc_I4_0),
                                                new CodeMatch(OpCodes.Ret))
                                            .Repeat(matcher => matcher.Set(OpCodes.Call, injectionCall))
                                            .InstructionEnumeration();
    }

    private static bool IsVisible()
    {
        if (!uGUI_PDA.main)
        {
            return false;
        }

        return Resolve<NitroxPDATabManager>().CustomTabs.TryGetValue(uGUI_PDA.main.currentTabType, out NitroxPDATab nitroxTab) && nitroxTab.KeepPingsVisible;
    }
}
