using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
///     Punching an attached bleeder (local player only) gives no dealer to LiveMixin.TakeDamage: marks it as the local player's attack so that
///     <see cref="CreatureHealthManager" /> sends it to the bleeder's simulating player.
/// </summary>
public sealed partial class Bleeder_OnHit_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bleeder t) => t.OnHit(default));

    public static void Prefix()
    {
        Resolve<CreatureHealthManager>().LocalAttackCount++;
    }

    public static void Finalizer()
    {
        Resolve<CreatureHealthManager>().LocalAttackCount--;
    }
}
