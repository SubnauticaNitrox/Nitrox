using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class LaunchRocket_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((LaunchRocket t) => t.OnHandClick(default));

    /*
     * 
     * LaunchRocket.SetLaunchStarted();
     * LaunchRocket_OnHandClick_Patch.BroadcastRocketLaunch(this);   <--- [INSERTED LINE]
     * [Removed all lines after]
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Call, Reflect.Method(() => LaunchRocket.SetLaunchStarted()))
                                            ])
                                            .Advance(1) // Skip the SetLaunchStarted because we don't want to manage all the jump labels that forward to it
                                            // Also this instruction doesn't block Rockets.RocketLaunch(GameObject) since that function doesn't check for LaunchRocket.launchStarted
                                            .RemoveInstructions(10)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastRocketLaunch(default)))
                                            ]).InstructionEnumeration();
    }

    private static void BroadcastRocketLaunch(LaunchRocket launchRocket)
    {
        Rocket rocket = launchRocket.RequireComponentInParent<Rocket>();
        Rockets rockets = Resolve<Rockets>();
        rockets.RequestRocketLaunch(rocket);
        rockets.RocketLaunch(rocket.gameObject);
    }
}
