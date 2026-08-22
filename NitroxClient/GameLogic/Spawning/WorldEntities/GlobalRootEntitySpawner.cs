using System.Collections;
using Nitrox.Model.DataStructures;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.Packets;
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
        GameObject gameObject = GameObjectExtensions.InstantiateWithId(prefab, entity.Id);
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
                SetupObjectInWaterPark(gameObject, largeWorldEntity, waterPark);

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

        // PipeSurfaceFloater (Floating Air Pump) is in SubnauticaMap.GLOBAL_ROOT_TECH_TYPES, so it always
        // spawns through this generic path -- but "deployed" is a purely client-local runtime flag vanilla
        // only ever sets via the original placer's own OnToolUseAnim() tool-use callback (PipeSurfaceFloater
        // doesn't derive from PlaceTool, so the block above never covers it). PipeSurfaceFloater.GetProvidesOxygen()
        // returns false unconditionally while !deployed, and the entire OxygenPipe chain rooted at this floater
        // (OxygenPipe.GetProvidesOxygen() ultimately asks GetRoot().GetProvidesOxygen()) silently stops
        // providing oxygen as a result -- for every client except the original placer's own still-running
        // session, i.e. any reconnect, and even the original placer after this GameObject's cell unloads and
        // reloads. deployed also gates WorldForces.handleGravity, so an undeployed floater drifts instead of
        // staying anchored. Any GlobalRootEntity of this TechType reaching this spawner is, by construction
        // (Items.cs.Dropped()), already placed -- so it's always correct to force this here.
        if (gameObject.TryGetComponent(out PipeSurfaceFloater pipeSurfaceFloater))
        {
            pipeSurfaceFloater.deployed = true;
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(pipeSurfaceFloater.rigidBody, true, false);
        }
    }

    public static void SetupObjectInWaterPark(GameObject gameObject, LargeWorldEntity largeWorldEntity, WaterPark waterPark)
    {
        // Fishes in water parks are GlobalRootEntities on server-side but client-side needs them at a regular cell level (not GlobalRoot)
        // initialCellLevel refers to the prefab's cell level which is the value we'll use
        largeWorldEntity.cellLevel = largeWorldEntity.initialCellLevel;

        gameObject.transform.SetParent(waterPark.itemsRoot, false);
        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            waterPark.AddItem(gameObject.EnsureComponent<Pickupable>());

            // While being fully loaded, the base is inactive so GameObject.SendMessage doesn't work and we need to execute their callbacks manually
            if (!Multiplayer.Main || Multiplayer.Main.InitialSyncCompleted)
            {
                return;
            }

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

    protected override bool SpawnsOwnChildren(GlobalRootEntity entity) => false;
}
