using System.Collections;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class GlobalRootEntitySpawner : SyncEntitySpawner<GlobalRootEntity>
{
    protected override IEnumerator SpawnAsync(GlobalRootEntity entity, TaskResult<Optional<GameObject>> result)
    {
        TaskResult<GameObject> gameObjectResult = new();
        yield return DefaultWorldEntitySpawner.CreateGameObject(entity.TechType.ToUnity(), entity.ClassId, entity.Id, gameObjectResult);
        GameObject gameObject = gameObjectResult.Get();

        SetupObject(entity, gameObject);

        result.Set(gameObject);
    }

    protected override bool SpawnSync(GlobalRootEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, entity.TechType.ToUnity(), entity.ClassId))
        {
            return false;
        }
        GameObject gameObject = GameObjectHelper.InstantiateWithId(prefab, entity.Id);
        SetupObject(entity, gameObject);

        result.Set(gameObject);
        return true;
    }

    private void SetupObject(GlobalRootEntity entity, GameObject gameObject)
    {
        LargeWorldEntity largeWorldEntity = gameObject.EnsureComponent<LargeWorldEntity>();
        largeWorldEntity.cellLevel = LargeWorldEntity.CellLevel.Global;
        
        LargeWorld.main.streamer.cellManager.RegisterEntity(largeWorldEntity);
        largeWorldEntity.Start();

        gameObject.transform.localPosition = entity.Transform.LocalPosition.ToUnity();
        gameObject.transform.localRotation = entity.Transform.LocalRotation.ToUnity();
        gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();

        if (entity.ParentId != null && NitroxEntity.TryGetComponentFrom(entity.ParentId, out Transform parentTransform))
        {
            // WaterParks have a child named "items_root" where the fish are put
            if (parentTransform.TryGetComponent(out WaterPark waterPark))
            {
                gameObject.transform.SetParent(waterPark.itemsRoot, false);
                using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
                {
                    waterPark.AddItem(gameObject.GetComponent<Pickupable>());

                    // While being fully loaded, the base is inactive so GameObject.SendMessage doesn't work and we need to execute their callbacks manually
                    if (Multiplayer.Main && !Multiplayer.Main.InitialSyncCompleted)
                    {
                        // Below are distinct incompatible cases
                        if (gameObject.TryGetComponent(out CreatureEgg creatureEgg) && !creatureEgg.insideWaterPark)
                        {
                            creatureEgg.OnAddToWaterPark();
                        }
                        else if (gameObject.TryGetComponent(out CuteFish cuteFish))
                        {
                            cuteFish.OnAddToWaterPark(null);
                        }
                        else if (gameObject.TryGetComponent(out CrabSnake crabSnake))
                        {
                            crabSnake.OnAddToWaterPark();
                        }
                    }
                }
            }
            else
            {
                gameObject.transform.SetParent(parentTransform, false);
            }
        }

        if (gameObject.GetComponent<PlaceTool>())
        {
            PlacedWorldEntitySpawner.AdditionalSpawningSteps(gameObject);
        }
    }

    protected override bool SpawnsOwnChildren(GlobalRootEntity entity) => false;
}
