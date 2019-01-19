using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning
{
    public class DefaultEntitySpawner : IEntitySpawner
    {
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent)
        {
            GameObject prefab;

            if (!PrefabDatabase.TryGetPrefab(entity.ClassId, out prefab))
            {
                prefab = CraftData.GetPrefabForTechType(entity.TechType, false);
                if (prefab == null)
                {
                    return Optional<GameObject>.Of(Utils.CreateGenericLoot(entity.TechType));
                }
            }

            GameObject gameObject = Utils.SpawnFromPrefab(prefab, null);
            gameObject.transform.position = entity.Position;
            gameObject.transform.localScale = entity.Scale;

            if (parent.IsPresent())
            {
                gameObject.transform.SetParent(parent.Get().transform, true);
            }

            gameObject.transform.localRotation = entity.Rotation;
            GuidHelper.SetNewGuid(gameObject, entity.Guid);
            gameObject.SetActive(true);
            // Makes movable objects movable... we can probably do this before the server sends the spawner packet?
            if(gameObject.GetComponent<Rigidbody>() ==  null)
            {
                gameObject.AddComponent<Rigidbody>();
            }

            LargeWorldEntity.Register(gameObject);
            CrafterLogic.NotifyCraftEnd(gameObject, entity.TechType);

            return Optional<GameObject>.Of(gameObject);
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}
