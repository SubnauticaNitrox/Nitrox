using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Vehicle_TorpedoShot_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => Vehicle.TorpedoShot(default, default, default));

    public static bool Prefix(Vehicle __instance, ref bool __result, ItemsContainer container, TorpedoType torpedoType, Transform muzzle)
    {
        // (almost) Exact code copy from Vehicle.TorpedoShot because it's impossible to make a transpiler for it without modifying most of the instructions
        // (the transpiler wouldn't be readable at all) so the best possibility is to copy the exact and out the created torpedo GameObject
        if (torpedoType == null || !container.DestroyItem(torpedoType.techType))
        {
            return false;
        }
        GameObject gameObject = GameObject.Instantiate(torpedoType.prefab);
        Bullet component = gameObject.GetComponent<SeamothTorpedo>();
        Transform aimingTransform = Player.main.camRoot.GetAimingTransform();
        Rigidbody componentInParent = muzzle.GetComponentInParent<Rigidbody>();
        Vector3 vector = componentInParent ? componentInParent.velocity : Vector3.zero;
        float num = Vector3.Dot(aimingTransform.forward, vector);
        component.Shoot(muzzle.position, aimingTransform.rotation, num, -1f);
        __result = true;

        // Broadcast code

        NitroxId bulletId = new();
        NitroxEntity.SetNewId(gameObject, bulletId);

        Resolve<IPacketSender>().Send(new TorpedoShot(bulletId, torpedoType.techType.ToDto(), muzzle.position.ToDto(), aimingTransform.rotation.ToDto(), num, -1));
        return false;
    }
}
