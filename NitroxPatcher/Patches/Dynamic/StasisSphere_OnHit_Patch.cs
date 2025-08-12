using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

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

        ushort? localPlayerId = Resolve<LocalPlayer>().PlayerId;
        // If the local player id isn't set then there's already a bigger problem/no problem and we can ignore that
        if (!localPlayerId.HasValue)
        {
            return true;
        }
        NitroxVector3 position = __instance.tr.position.ToDto();
        NitroxQuaternion rotation = __instance.tr.rotation.ToDto();
        // Calculate the chargeNormalized value which was passed to StasisSphere.Shoot
        float chargeNormalized = Mathf.Unlerp(__instance.minRadius, __instance.maxRadius, __instance.radius);

        Resolve<IPacketSender>().Send(new StasisSphereHit(localPlayerId.Value, position, rotation, chargeNormalized, __instance._consumption));
        return true;
    }
}
