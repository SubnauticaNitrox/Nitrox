using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents remote stasis sphere from triggering hit effets (they're triggered with packets)
/// </summary>
public sealed partial class StasisSphere_OnHit_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StasisSphere t) => t.OnHit(default));

    private static ushort LocalPlayerId => Resolve<LocalPlayer>().PlayerId;

    public static void Prefix(StasisSphere __instance)
    {
        if (__instance.GetComponent<BulletManager.RemotePlayerBullet>())
        {
            return;
        }

        NitroxVector3 position = __instance.tr.position.ToDto();
        NitroxQuaternion rotation = __instance.tr.rotation.ToDto();
        // Calculate the chargeNormalized value which was passed to StasisSphere.Shoot
        float chargeNormalized = Mathf.Unlerp(__instance.minRadius, __instance.maxRadius, __instance.radius);

        Resolve<IPacketSender>().Send(new StasisSphereHit(LocalPlayerId, position, rotation, chargeNormalized, __instance._consumption));
        return;
    }
}
