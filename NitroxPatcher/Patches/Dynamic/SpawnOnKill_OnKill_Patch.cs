using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Synchronizes entities that Spawn something when they are killed, e.g. Coral Disks.
/// </summary>
public class SpawnOnKill_OnKill_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpawnOnKill t) => t.OnKill());

    public static bool Prefix(SpawnOnKill __instance)
    {
        // ORIGINAL CODE
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.prefabToSpawn, __instance.transform.position, __instance.transform.rotation);
        if (__instance.randomPush)
        {
            Rigidbody component = gameObject.GetComponent<Rigidbody>();
            if (component)
            {
                component.AddForce(UnityEngine.Random.onUnitSphere * 1.4f, ForceMode.Impulse);
            }
        }

        // get the ID of the destroyed object
        NitroxId destroyedEntityId = NitroxEntity.GetId(__instance.gameObject);
        // Says that the entity doesn't exist anymore  --  Without this, the gameobject is not updated. It will not be pickuppable after that.
        Resolve<IPacketSender>().Send(new EntityDestroyed(destroyedEntityId));
        // Give an ID to the new game object
        NitroxEntity.SetNewId(gameObject, destroyedEntityId);
        // Drop the item in the game
        Resolve<Items>().Dropped(gameObject, CraftData.GetTechType(gameObject));

        // Prevent the original function to happen twice, so avoid duplication of the object on client side
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
