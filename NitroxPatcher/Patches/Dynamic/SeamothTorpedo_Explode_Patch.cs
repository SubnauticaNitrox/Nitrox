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
/// Broadcasts torpedo explosion
/// </summary>
public sealed partial class SeamothTorpedo_Explode_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeamothTorpedo t) => t.Explode());

    public static void Prefix(SeamothTorpedo __instance)
    {
        if (__instance.GetComponent<BulletManager.RemotePlayerBullet>() || !__instance.TryGetNitroxId(out NitroxId bulletId))
        {
            return;
        }
        // When Bullet.Update detects a collision with the spherecast, it calls Deactivate which sets _energy to 0
        // So Bullet.OnEnergyDepleted is also called by Bullet.Update, therefore this patch is executed twice
        // We can mark the torpedo as if it was a remote torpedo so we don't send a hit packet twice
        __instance.gameObject.AddComponent<BulletManager.RemotePlayerBullet>();

        NitroxVector3 position = __instance.tr.position.ToDto();
        NitroxQuaternion rotation = __instance.tr.rotation.ToDto();

        Resolve<IPacketSender>().Send(new TorpedoHit(bulletId, position, rotation));
    }
}
