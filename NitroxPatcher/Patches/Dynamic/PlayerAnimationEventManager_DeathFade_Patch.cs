using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents a remote player's death animation from fading the local player's screen.
/// </summary>
public sealed partial class PlayerAnimationEventManager_DeathFade_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PlayerAnimationEventManager t) => t.DeathFade());

    public static bool Prefix(PlayerAnimationEventManager __instance) =>
        !__instance.GetComponentInParent<RemotePlayerIdentifier>(true);
}