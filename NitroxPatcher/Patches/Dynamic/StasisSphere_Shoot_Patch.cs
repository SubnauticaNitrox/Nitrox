using System.Reflection;
using Nitrox.Model.Core;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class StasisSphere_Shoot_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StasisSphere t) => t.Shoot(default, default, default, default, default));

    private static SessionId? LocalSessionId => Resolve<LocalPlayer>().SessionId;

    public static void Prefix(StasisSphere __instance, Vector3 position, Quaternion rotation, float speed, float lifeTime, float chargeNormalized)
    {
        if (__instance.GetComponent<BulletManager.RemotePlayerBullet>() || !LocalSessionId.HasValue)
        {
            return;
        }

        Resolve<IPacketSender>().Send(new StasisSphereShot(LocalSessionId.Value, position.ToDto(), rotation.ToDto(), speed, lifeTime, chargeNormalized));
    }
}
