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
            GameObject gameObject = SpawnViaTechTypePrefab(entity) ?? SpawnViaPrefabDatabase(entity) ?? SpawnBlackBox(entity);
            
            return Optional<GameObject>.Of(gameObject);
        }
        
        private GameObject SpawnViaTechTypePrefab(Entity entity)
        {
            GameObject prefabForTechType = CraftData.GetPrefabForTechType(entity.TechType, false);

            if (prefabForTechType == null)
            {
                return null;
            }

            GameObject gameObject = Utils.SpawnFromPrefab(prefabForTechType, null);
            gameObject.transform.position = entity.Position;
            gameObject.transform.localRotation = entity.Rotation;
            GuidHelper.SetNewGuid(gameObject, entity.Guid);
            gameObject.SetActive(true);
            LargeWorldEntity.Register(gameObject);
            CrafterLogic.NotifyCraftEnd(gameObject, entity.TechType);

            return gameObject;
        }
        
        private GameObject SpawnViaPrefabDatabase(Entity entity)
        {
            GameObject prefab;

            if (PrefabDatabase.TryGetPrefab(entity.ClassId, out prefab))
            {
                GameObject gameObject = UWE.Utils.InstantiateDeactivated(prefab, entity.Position, entity.Rotation);
                gameObject.transform.SetParent(null, false);
                gameObject.SetActive(true);

                LargeWorldEntity component = gameObject.GetComponent<LargeWorldEntity>();
                component.cellLevel = (LargeWorldEntity.CellLevel)entity.AbsoluteEntityCell.Level;
                LargeWorld.main.streamer.cellManager.RegisterEntity(component);
                LargeWorldEntity.Register(gameObject);
                GuidHelper.SetNewGuid(gameObject, entity.Guid);

                return gameObject;
            }

            return null;
        }

        private GameObject SpawnBlackBox(Entity entity)
        {
            return Utils.CreateGenericLoot(entity.TechType);
        }
    }
}
