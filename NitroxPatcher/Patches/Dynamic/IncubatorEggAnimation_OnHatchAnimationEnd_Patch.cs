using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Handles cleanup of temporary babies created for animation on non-simulating players.
/// Only the real networked baby (from simulating player) should call SwimToMother().
/// Temporary babies are destroyed after the animation completes.
/// </summary>
public sealed partial class IncubatorEggAnimation_OnHatchAnimationEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((IncubatorEggAnimation t) => t.OnHatchAnimationEnd());

    public static bool Prefix(IncubatorEggAnimation __instance)
    {
        // Safety check - ensure we have a baby reference
        if (!__instance.baby)
        {
            return true; // Let original method handle this case
        }

        // Check if this is a temporary baby (created for animation only)
        if (__instance.baby.name.Contains("_NitroxTemporary"))
        {
            // For temporary babies, we only want to end the cinematic mode and cleanup
            // Don't call SwimToMother() as that should only happen for the real networked baby
            __instance.baby.cinematicController.SetCinematicMode(false);
            
            // Use reflection to safely set the animationActive field to false
            FieldInfo animationActiveField = typeof(IncubatorEggAnimation).GetField("animationActive", BindingFlags.NonPublic | BindingFlags.Instance);
            if (animationActiveField != null)
            {
                animationActiveField.SetValue(__instance, false);
            }
            
            // Destroy the temporary baby - the real networked one will be spawned via server broadcast
            Object.Destroy(__instance.baby.gameObject);
            return false; // Skip the original method
        }

        // For real networked babies, let the original method run (which calls SwimToMother)
        return true;
    }
}