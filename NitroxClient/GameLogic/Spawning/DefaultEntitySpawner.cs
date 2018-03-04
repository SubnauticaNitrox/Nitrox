using NitroxModel.DataStructures.GameLogic;
using NitroxClient.GameLogic.Helper;
using UnityEngine;
using NitroxModel.DataStructures.Util;
using UWE;

namespace NitroxClient.GameLogic.Spawning
{
    public class DefaultEntitySpawner : IEntitySpawner
    {
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent)
        {
            GameObject prefabForTechType = CraftData.GetPrefabForTechType(entity.TechType, false);

            if (prefabForTechType == null && !PrefabDatabase.TryGetPrefab(entity.ClassId, out prefabForTechType))
            {
                return Optional<GameObject>.Of(Utils.CreateGenericLoot(entity.TechType));
            }

            GameObject gameObject = Utils.SpawnFromPrefab(prefabForTechType, null);
            gameObject.transform.position = entity.Position;
            gameObject.transform.localRotation = entity.Rotation;
            GuidHelper.SetNewGuid(gameObject, entity.Guid);
            gameObject.SetActive(true);
            LargeWorldEntity.Register(gameObject);
            CrafterLogic.NotifyCraftEnd(gameObject, entity.TechType);

            return Optional<GameObject>.Of(gameObject);
        }
    }
}
