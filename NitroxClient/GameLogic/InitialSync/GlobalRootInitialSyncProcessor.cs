using System.Collections;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync;

/// <summary>
///     Makes sure players can be spawned in entities in the global root (such as vehicles/escape pod).
/// </summary>
/// <remarks>
///     This allows for:<br/>
///      - vehicles to use equipment
///      - other players to be set as drivers of some vehicle
/// </remarks>
public sealed class GlobalRootInitialSyncProcessor : InitialSyncProcessor
{
    private readonly Entities entities;
    private readonly Vehicles vehicles;
    private readonly PlayerManager playerManager;
    private readonly BulletManager bulletManager;

    public GlobalRootInitialSyncProcessor(Entities entities, Vehicles vehicles, PlayerManager playerManager, BulletManager bulletManager)
    {
        this.entities = entities;
        this.vehicles = vehicles;
        this.playerManager = playerManager;
        this.bulletManager = bulletManager;

        // As we migrate systems over to entities, we want to ensure the required components are in place to spawn these entities.
        // For example, migrating inventories to the entity system requires players are spawned in the world before we try to add
        // inventory items to them.  Eventually, all of the below processors will become entities on their own
        AddDependency<PlayerInitialSyncProcessor>();
        AddDependency<RemotePlayerInitialSyncProcessor>();
        AddDependency<StoryGoalInitialSyncProcessor>();

        AddStep(WorldSettledForBuildings);
        AddStep(SpawnEntities);
        AddStep(RestoreDrivers);
    }

    public IEnumerator WorldSettledForBuildings(InitialPlayerSync packet)
    {
        yield return new WaitUntil(LargeWorldStreamer.main.IsWorldSettled);
        // Make sure all building-related prefabs are fully loaded (happen to bug when launching multiple clients locally)
        yield return Base.InitializeAsync();
        yield return BaseGhost.InitializeAsync();
        yield return BaseDeconstructable.InitializeAsync();
        yield return VirtualCyclops.InitializeConstructablesCache();
        yield return bulletManager.Initialize();

        BuildingHandler.Main.InitializeOperations(packet.BuildOperationIds);
    }

    public IEnumerator SpawnEntities(InitialPlayerSync packet)
    {
        Log.Info($"Received initial sync packet with {packet.GlobalRootEntities.Count} global root entities");
        yield return entities.SpawnBatchAsync(packet.GlobalRootEntities);
    }

    public void RestoreDrivers(InitialPlayerSync packet)
    {
        // At this step, vehicles have been spawned already (by SpawnEntities)
        foreach (PlayerContext playerContext in packet.OtherPlayers)
        {
            if (playerContext.DrivingVehicle != null)
            {
                Log.Info($"Restoring driver state of {playerContext.PlayerName} in {playerContext.DrivingVehicle}");
                vehicles.SetOnPilotMode(playerContext.DrivingVehicle, playerContext.PlayerId, true);
                if (playerManager.TryFind(playerContext.PlayerId, out RemotePlayer remotePlayer))
                {
                    // As remote players are still driving, they aren't updating their IsUnderwater state so AnimationSender.Update
                    // isn't going to send a packet. Therefore we need to set this by hand
                    remotePlayer.UpdateAnimationAndCollider(AnimChangeType.UNDERWATER, AnimChangeState.OFF);
                }
            }
        }
    }
}
