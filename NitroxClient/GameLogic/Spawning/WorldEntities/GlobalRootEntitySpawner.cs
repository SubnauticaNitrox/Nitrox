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
using UWE;

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
                    waterPark.AddItem(gameObject.EnsureComponent<Pickupable>());

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
                            // This callback interacts with an animator, but this behaviour needs to be initialized (probably during Start) before it can be modified
                            IEnumerator PostponedCallback()
                            {
                                yield return new WaitUntil(() => !crabSnake || crabSnake.animationController.animator.isInitialized);                                
                                if (crabSnake)
                                {
                                    crabSnake.OnAddToWaterPark();
                                }
                            }
                            CoroutineHost.StartCoroutine(PostponedCallback());
                        }
                    }
                }

                // TODO: When metadata is reworked (it'll be possible to give different metadatas to the same entity)
                // this will no longer be needed because the entity metadata will set this to false accordingly

                // If fishes are in a WaterPark, it means that they were once picked up
                if (gameObject.TryGetComponent(out CreatureDeath creatureDeath))
                {
                    // This is set to false when picking up a fish or when a fish is born in the WaterPark
                    creatureDeath.respawn = false;
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
