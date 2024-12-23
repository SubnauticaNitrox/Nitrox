using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class StasisSphere_Shoot_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StasisSphere t) => t.Shoot(default, default, default, default, default));

    private static ushort? LocalPlayerId => Resolve<LocalPlayer>().PlayerId;

    public static void Prefix(StasisSphere __instance, Vector3 position, Quaternion rotation, float speed, float lifeTime, float chargeNormalized)
    {
        if (__instance.GetComponent<BulletManager.RemotePlayerBullet>() || !LocalPlayerId.HasValue)
        {
            return;
        }

        Resolve<IPacketSender>().Send(new StasisSphereShot(LocalPlayerId.Value, position.ToDto(), rotation.ToDto(), speed, lifeTime, chargeNormalized));
    }
}
