using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents remote torpedos from acquiring target on their own, and broadcast the new target for simulated torpedos
/// </summary>
public sealed partial class SeamothTorpedo_RepeatingTargeting_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeamothTorpedo t) => t.RepeatingTargeting());

    public static bool Prefix(SeamothTorpedo __instance)
    {
        return !__instance.GetComponent<BulletManager.RemotePlayerBullet>();
    }

    public static void Postfix(SeamothTorpedo __instance)
    {
        // This function's last iteration is marked by SeamothTorpedo.homingTorpedo being defined
        if (__instance.GetComponent<BulletManager.RemotePlayerBullet>() || !__instance.homingTarget ||
            !__instance.TryGetNitroxId(out NitroxId bulletId) || !__instance.homingTarget.TryGetNitroxId(out NitroxId targetId))
        {
            return;
        }

        NitroxVector3 position = __instance.tr.position.ToDto();
        NitroxQuaternion rotation = __instance.tr.rotation.ToDto();

        Resolve<IPacketSender>().Send(new TorpedoTargetAcquired(bulletId, targetId, position, rotation));
    }
}
