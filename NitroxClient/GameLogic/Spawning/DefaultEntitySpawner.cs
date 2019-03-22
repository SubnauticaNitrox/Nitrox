using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.Helper;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning
{
    public class DefaultEntitySpawner : IEntitySpawner
    {
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent)
        {
            TechType techType = entity.TechType.Enum();
            GameObject prefab;

            if (!PrefabDatabase.TryGetPrefab(entity.ClassId, out prefab))
            {
                prefab = CraftData.GetPrefabForTechType(techType, false);
                if (prefab == null)
                {
                    return Optional<GameObject>.Of(Utils.CreateGenericLoot(techType));
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

            LargeWorldEntity.Register(gameObject);
            CrafterLogic.NotifyCraftEnd(gameObject, techType);

            return Optional<GameObject>.Of(gameObject);
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}
