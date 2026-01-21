using System.Linq;
using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// When a Sea Emperor baby is spawned on a remote client (via server broadcast),
/// it needs to swim to mother. The simulating player handles this via the egg animation
/// callback, but remote players receive the baby as a standalone entity.
/// 
/// This patch ensures the baby swims to mother if:
/// 1. SeaEmperor.main exists (we're in the prison aquarium)
/// 2. The baby has no parent (not attached to an egg - means it's a remote spawn)
/// 3. The baby doesn't already have a swim target (not already swimming)
/// 4. The baby is not a temporary animation baby
/// </summary>
public sealed partial class SeaEmperorBaby_Start_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaEmperorBaby t) => t.Start());

    public static void Postfix(SeaEmperorBaby __instance)
    {
        // Only trigger swim to mother for remotely spawned babies
        if (!SeaEmperor.main)
        {
            return;
        }

        // Skip temporary babies created for animation
        if (__instance.name.Contains(IncubatorEggAnimation_OnHatchAnimationEnd_Patch.TEMPORARY_BABY_MARKER))
        {
            return;
        }

        // If the baby has a parent, it was spawned by the local hatching sequence
        // The animation system will handle calling SwimToMother() via OnHatchAnimationEnd()
        if (__instance.transform.parent != null)
        {
            return;
        }

        // Check if this baby already has a swim target (already being handled)
        if (__instance.swimToTarget != null && __instance.swimToTarget.target != null)
        {
            return;
        }

        // This is a remotely spawned baby - make it swim to mother
        // Assign a baby ID based on how many babies are already registered
        int babyId = SeaEmperor.main.GetBabies().Count();
        __instance.SetId(babyId);
        __instance.SwimToMother();
    }
}
