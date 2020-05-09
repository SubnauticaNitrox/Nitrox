using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.Helper;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning
{
    public class DefaultEntitySpawner : IEntitySpawner
    {
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
        {
            TechType techType = entity.TechType.Enum();

            GameObject gameObject = CreateGameObject(techType, entity.ClassId);
            gameObject.transform.position = entity.Transform.Position;
            gameObject.transform.rotation = entity.Transform.Rotation;
            gameObject.transform.localScale = entity.Transform.LocalScale;

            NitroxEntity.SetNewId(gameObject, entity.Id);
            CrafterLogic.NotifyCraftEnd(gameObject, techType);

            if (parent.HasValue && !parent.Value.GetComponent<LargeWorldEntityCell>())
            {
                LargeWorldEntity.Register(gameObject); // This calls SetActive on the GameObject
            }
            else if (gameObject.GetComponent<LargeWorldEntity>() != null && gameObject.transform.parent == null)
            {
                gameObject.transform.SetParent(cellRoot.liveRoot.transform, true);
                LargeWorldEntity.Register(gameObject);
            }
            else
            {
                gameObject.SetActive(true);
            }

            if (parent.HasValue)
            {
                gameObject.transform.SetParent(parent.Value.transform, true);
            }

            Optional<EntityMetadataProcessor> metadataProcessor = EntityMetadataProcessor.FromMetaData(entity.Metadata);

            if (metadataProcessor.HasValue)
            {
                metadataProcessor.Value.ProcessMetadata(gameObject, entity.Metadata);
            }

            return Optional.Of(gameObject);
        }

        private GameObject CreateGameObject(TechType techType, string classId)
        {
            GameObject prefab;
            IPrefabRequest prefabRequest = PrefabDatabase.GetPrefabAsync(classId);
            if (!prefabRequest.TryGetPrefab(out prefab))
            {
                prefab = CraftData.GetPrefabForTechType(techType, false);

                if (prefab == null)
                {
                    return Utils.CreateGenericLoot(techType);
                }
            }

            return Utils.SpawnFromPrefab(prefab, null);
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}
