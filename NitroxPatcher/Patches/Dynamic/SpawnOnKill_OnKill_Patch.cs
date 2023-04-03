using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    /// <summary>
    /// Synchronizes entities that Spawn something when they are killed, e.g. Coral Disks.
    /// </summary>
    internal class SpawnOnKill_OnKill_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpawnOnKill t) => t.OnKill());

        public static bool Prefix(SpawnOnKill __instance)
        {
            // I did this for safety reason, but I don't think it's necessary
            if (__instance != null)
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
                
                // Get the ID of the new game object
                NitroxId newEntityId = NitroxEntity.GetId(gameObject);

                // get the ID of the destroyed object
                NitroxId destroyedEntityId = NitroxEntity.GetId(__instance.gameObject);
                // Send packet telling that it was destroyed to the server
                Resolve<IPacketSender>().Send(new EntityDestroyed(destroyedEntityId));

                // Give a new ID to the new game object
                NitroxEntity.SetNewId(gameObject, newEntityId);
                // Drop the item in the game
                Resolve<Items>().Dropped(gameObject, CraftData.GetTechType(gameObject));

                // Prevent the original function to happen twice, so avoid duplication of the object on client side
                return false;
            }
            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
