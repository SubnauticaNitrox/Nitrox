using System.Collections;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class VehicleWorldEntitySpawner : IWorldEntitySpawner
{
    // The constructor has mixed results when the remote player is a long distance away.  UWE even has a built in distance tracker to ensure
    // that they are within allowed range.  However, this range is a bit restrictive. We will allow constructor spawning up to a specified 
    // distance - anything more will simply use world spawning (no need to play the animation anyways).
    private const float ALLOWED_CONSTRUCTOR_DISTANCE = 100.0f;

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    { 
        VehicleWorldEntity vehicleEntity = (VehicleWorldEntity)entity;

        bool withinConstructorSpawnWindow = (DayNightCycle.main.timePassedAsFloat - vehicleEntity.ConstructionTime) < GetCraftDuration(vehicleEntity.TechType.ToUnity());
        Optional<GameObject> spawnerObj = NitroxEntity.GetObjectFrom(vehicleEntity.SpawnerId);

        if (withinConstructorSpawnWindow && spawnerObj.HasValue)
        {
            Constructor constructor = spawnerObj.Value.GetComponent<Constructor>();
            float distance = (constructor.transform.position - Player.main.transform.position).magnitude;
            bool withinDistance = distance <= ALLOWED_CONSTRUCTOR_DISTANCE;

            if (constructor && withinDistance)
            {
                MobileVehicleBay.TransmitLocalSpawns = false;
                yield return SpawnViaConstructor(vehicleEntity, constructor, result);
                MobileVehicleBay.TransmitLocalSpawns = true;
                yield break;
            }
        }

        yield return SpawnInWorld(vehicleEntity, result, parent);            
    }

    private IEnumerator SpawnInWorld(VehicleWorldEntity vehicleEntity, TaskResult<Optional<GameObject>> result, Optional<GameObject> parent)
    {
        TechType techType = vehicleEntity.TechType.ToUnity();
        GameObject gameObject = null;

        if (techType == TechType.Cyclops)
        {
            GameObject prefab = null;
            LightmappedPrefabs.main.RequestScenePrefab("cyclops", (go) => prefab = go);
            yield return new WaitUntil(() => prefab != null);
            SubConsoleCommand.main.OnSubPrefabLoaded(prefab);
            gameObject = SubConsoleCommand.main.GetLastCreatedSub();
        }
        else
        {
            CoroutineTask<GameObject> techPrefabCoroutine = CraftData.GetPrefabForTechTypeAsync(techType, false);
            yield return techPrefabCoroutine;
            GameObject techPrefab = techPrefabCoroutine.GetResult();
            gameObject = Utils.SpawnPrefabAt(techPrefab, null, vehicleEntity.Transform.Position.ToUnity());
            Validate.NotNull(gameObject, $"{nameof(VehicleWorldEntitySpawner)}: No prefab for tech type: {techType}");
            Vehicle vehicle = gameObject.GetComponent<Vehicle>();

            if (vehicle)
            {
                vehicle.LazyInitialize();
            }
        }

        AddCinematicControllers(gameObject);

        gameObject.transform.position = vehicleEntity.Transform.Position.ToUnity();
        gameObject.transform.rotation = vehicleEntity.Transform.Rotation.ToUnity();
        gameObject.SetActive(true);
        gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);

        CrafterLogic.NotifyCraftEnd(gameObject, CraftData.GetTechType(gameObject));
        Rigidbody rigidBody = gameObject.RequireComponent<Rigidbody>();
        rigidBody.isKinematic = false;

        yield return Yielders.WaitForEndOfFrame;

        RemoveConstructionAnimations(gameObject);

        yield return Yielders.WaitForEndOfFrame;

        Vehicles.RemoveNitroxEntityTagging(gameObject);

        NitroxEntity.SetNewId(gameObject, vehicleEntity.Id);

        if (parent.HasValue)
        {
            DockVehicle(gameObject, parent.Value);
        }

        result.Set(gameObject);
    }

    private IEnumerator SpawnViaConstructor(VehicleWorldEntity vehicleEntity, Constructor constructor, TaskResult<Optional<GameObject>> result)
    {
        if (!constructor.deployed)
        {
            constructor.Deploy(true);
        }

        float craftDuration = GetCraftDuration(vehicleEntity.TechType.ToUnity()) - (DayNightCycle.main.timePassedAsFloat - vehicleEntity.ConstructionTime);

        ConstructorInput crafter = constructor.gameObject.RequireComponentInChildren<ConstructorInput>(true);

        yield return crafter.OnCraftingBeginAsync(vehicleEntity.TechType.ToUnity(), craftDuration);

        GameObject constructedObject = MobileVehicleBay.MostRecentlyCrafted;
        Validate.IsTrue(constructedObject, $"Could not find constructed object from MobileVehicleBay {constructor.gameObject.name}");

        NitroxEntity.SetNewId(constructedObject, vehicleEntity.Id);

        AddCinematicControllers(constructedObject);

        result.Set(constructedObject);
        yield break;
    }

    /// <summary>
    ///   For scene objects like cyclops, PlayerCinematicController Start() will not be called to add Cinematic reference.
    /// </summary>
    private void AddCinematicControllers(GameObject gameObject)
    {
        if (gameObject.GetComponent<MultiplayerCinematicReference>())
        {
            return;
        }

        PlayerCinematicController[] controllers = gameObject.GetComponentsInChildren<PlayerCinematicController>();

        if (controllers.Length == 0)
        {
            return;
        }

        MultiplayerCinematicReference reference = gameObject.AddComponent<MultiplayerCinematicReference>();

        foreach (PlayerCinematicController controller in controllers)
        {
            reference.AddController(controller);
        }
    }

    /// <summary>
    ///  When loading in vehicles, they still briefly have their blue crafting animation playing.  Force them to stop.
    /// </summary>
    private void RemoveConstructionAnimations(GameObject gameObject)
    {
        VFXConstructing[] vfxConstructions = gameObject.GetComponentsInChildren<VFXConstructing>();
        
        foreach (VFXConstructing vfxConstructing in vfxConstructions)
        {
            vfxConstructing.EndGracefully();
        }
    }

    private void DockVehicle(GameObject gameObject, GameObject parent)
    {
        Vehicle vehicle = gameObject.GetComponent<Vehicle>();

        if (!vehicle)
        {
            Log.Info($"Could not find vehicle component on docked vehicle {gameObject.name}");
            return;
        }

        VehicleDockingBay dockingBay = parent.GetComponentInChildren<VehicleDockingBay>();

        if (!dockingBay)
        {
            Log.Info($"Could not find VehicleDockingBay component on dock object {parent.name}");
            return;
        }

        dockingBay.DockVehicle(vehicle);        
    }

    public bool SpawnsOwnChildren()
    {
        return false;
    }
    
    private float GetCraftDuration(TechType techType)
    {
        // UWE hard codes the build times into if/else logic inside ConstructorInput.Craft().

        switch(techType)
        {
            case TechType.Seamoth:
                return 10f;
            case TechType.Exosuit:
                return 10f;
            case TechType.Cyclops:
                return 20f;
            case TechType.RocketBase:
                return 25f;
        }

        return 10f;
    }
}
