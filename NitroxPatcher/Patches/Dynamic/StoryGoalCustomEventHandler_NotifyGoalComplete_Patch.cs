using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// When the gun deactivation story goal is received from another player,
/// we need to set gunDisabled = true so rocket launch works.
/// The base game's NotifyGoalComplete doesn't do this - it only sets gunDisabled in DisableGun() or Awake().
/// </summary>
public sealed partial class StoryGoalCustomEventHandler_NotifyGoalComplete_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StoryGoalCustomEventHandler t) => t.NotifyGoalComplete(default));

    public static void Postfix(StoryGoalCustomEventHandler __instance, string key)
    {
        // The goal key for gun deactivation - same key checked in the original NotifyGoalComplete
        if (string.Equals(key, "Goal_Disable_Gun", StringComparison.OrdinalIgnoreCase))
        {
            __instance.gunDisabled = true;
        }
    }
}
