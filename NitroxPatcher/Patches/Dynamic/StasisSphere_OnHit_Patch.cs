using System.Reflection;
using Nitrox.Model.Core;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents remote stasis sphere from triggering hit effets (they're triggered with packets).
/// Broadcasts local player's stasis sphere hits
/// </summary>
public sealed partial class StasisSphere_OnHit_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StasisSphere t) => t.OnHit(default));

    public static bool Prefix(StasisSphere __instance)
    {
        if (__instance.GetComponent<BulletManager.RemotePlayerBullet>())
        {
            return false;
        }

        SessionId? localSessionId = Resolve<LocalPlayer>().SessionId;
        // If the local player id isn't set then there's already a bigger problem/no problem and we can ignore that
        if (!localSessionId.HasValue)
        {
            return true;
        }
        NitroxVector3 position = __instance.tr.position.ToDto();
        NitroxQuaternion rotation = __instance.tr.rotation.ToDto();
        // Calculate the chargeNormalized value which was passed to StasisSphere.Shoot
        float chargeNormalized = Mathf.Unlerp(__instance.minRadius, __instance.maxRadius, __instance.radius);

        Resolve<IPacketSender>().Send(new StasisSphereHit(localSessionId.Value, position, rotation, chargeNormalized, __instance._consumption));
        return true;
    }
}
