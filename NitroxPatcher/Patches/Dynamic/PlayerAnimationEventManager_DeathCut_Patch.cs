using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents a remote player's death animation from cutting the local player's screen to black.
/// </summary>
public sealed partial class PlayerAnimationEventManager_DeathCut_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PlayerAnimationEventManager t) => t.DeathCut());

    public static bool Prefix(PlayerAnimationEventManager __instance) =>
        !__instance.GetComponentInParent<RemotePlayerIdentifier>(true);
}