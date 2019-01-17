using System;
using System.Reflection;
using Harmony;
using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxClient.GameLogic;

// TODO: Temporarily persistent to run before everything.  When we migrate the patch hook to an early point then make this non-persistent
namespace NitroxPatcher.Patches.Persistent
{
    class ProtobufSerializer_DeserializeIntoGameObject_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ProtobufSerializer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("DeserializeIntoGameObject", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public static void Postfix(ProtobufSerializer.GameObjectData goData, UniqueIdentifier uid)
        {
            bool isMultiplayer = (Multiplayer.Main != null && Multiplayer.Main.IsMultiplayer());

            if (isMultiplayer && SerializationHelper.BLOCK_HAND_PLACED_DESERIALIZATION && SpawnedWithoutServersPermission(goData, uid.gameObject))
            {
                UnityEngine.Object.Destroy(uid.gameObject);
            }
        }

        private static bool SpawnedWithoutServersPermission(ProtobufSerializer.GameObjectData goData, GameObject gameObject)
        {
            NitroxEntity serverEntity = gameObject.GetComponent<NitroxEntity>();

            if(serverEntity)
            {
                // if we have a NitroxEntity then the server is aware of this entity.
                return false;
            }

            UniqueIdentifier identifier = gameObject.GetComponent<UniqueIdentifier>();
            Entities entities = NitroxServiceLocator.LocateService<Entities>();

            if (identifier != null && entities.WasSpawnedByServer(identifier.Id))
            {
                // Looks like this ran through the main entity spawning code - the server knows about it.
                return false;
            }

            // We've exhausted all mechanisms of entity detection - this doesn't appear to be an entity that the server is aware of.
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
