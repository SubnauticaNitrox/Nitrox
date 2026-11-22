using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
///     The claw arm's punch gives no dealer to LiveMixin.TakeDamage and only happens for the local pilot: marks it as the local player's attack so that
///     <see cref="CreatureHealthManager" /> sends it to the hit creature's simulating player.
/// </summary>
public sealed partial class ExosuitClawArm_OnHit_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitClawArm t) => t.OnHit());

    public static void Prefix()
    {
        Resolve<CreatureHealthManager>().LocalAttackCount++;
    }

    public static void Finalizer()
    {
        Resolve<CreatureHealthManager>().LocalAttackCount--;
    }
}
