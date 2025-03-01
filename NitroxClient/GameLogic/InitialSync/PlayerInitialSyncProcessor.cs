using System.Collections;
using System.Text;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync;

/// <summary>
///     Makes sure the player is configured.
/// </summary>
/// <remarks>
///     This allows the player to:<br/>
///      - use equipment
/// </remarks>
public sealed class PlayerInitialSyncProcessor : InitialSyncProcessor
{
    private readonly Items item;
    private readonly ItemContainers itemContainers;
    private readonly LocalPlayer localPlayer;

    public PlayerInitialSyncProcessor(Items item, ItemContainers itemContainers, LocalPlayer localPlayer)
    {
        this.item = item;
        this.itemContainers = itemContainers;
        this.localPlayer = localPlayer;

        AddStep(sync => SetupEscapePod(sync.FirstTimeConnecting));
        AddStep(sync => SetPlayerPermissions(sync.Permissions));
        AddStep(sync => SetPlayerIntroCinematicMode(sync.IntroCinematicMode));
        AddStep(sync => SetPlayerGameObjectId(sync.PlayerGameObjectId));
        AddStep(sync => AddStartingItemsToPlayer(sync.FirstTimeConnecting));
        AddStep(sync => SetPlayerStats(sync.PlayerStatsData));
        AddStep(sync => SetPlayerGameMode(sync.GameMode));
        AddStep(sync => ApplySettings(sync.KeepInventoryOnDeath, sync.SessionSettings.FastHatch, sync.SessionSettings.FastGrow));
    }

    private void SetPlayerPermissions(Perms permissions)
    {
        localPlayer.Permissions = permissions;
    }

    private void SetPlayerIntroCinematicMode(IntroCinematicMode introCinematicMode)
    {
        if (localPlayer.IntroCinematicMode < introCinematicMode)
        {
            localPlayer.IntroCinematicMode = introCinematicMode;
            Log.Info($"Received initial sync player IntroCinematicMode: {introCinematicMode}");
        }
    }

    private static void SetPlayerGameObjectId(NitroxId id)
    {
        EcoTarget playerEcoTarget = Player.mainObject.AddComponent<EcoTarget>();
        playerEcoTarget.SetTargetType(RemotePlayer.PLAYER_ECO_TARGET_TYPE);

        NitroxEntity.SetNewId(Player.mainObject, id);
        Log.Info($"Received initial sync player GameObject Id: {id}");
    }

    private void SetupEscapePod(bool firstTimeConnecting)
    {
        EscapePod escapePod = EscapePod.main;
        if (escapePod)
        {
            Log.Info($"Setting up escape pod, FirstTimeConnecting: {firstTimeConnecting}");

            escapePod.bottomHatchUsed = !firstTimeConnecting;
            escapePod.topHatchUsed = !firstTimeConnecting;

            // Call code we suppressed inside EscapePodFirstUseCinematicsController_OnSceneObjectsLoaded_Patch
            EscapePodFirstUseCinematicsController cinematicController = escapePod.GetComponentInChildren<EscapePodFirstUseCinematicsController>(true);
            if (cinematicController)
            {
                cinematicController.Initialize();
            }
        }
    }

    private IEnumerator AddStartingItemsToPlayer(bool firstTimeConnecting)
    {
        if (firstTimeConnecting)
        {
            if (!Player.main.TryGetIdOrWarn(out NitroxId localPlayerId))
            {
                yield break;
            }

            foreach (TechType techType in LootSpawner.main.GetEscapePodStorageTechTypes())
            {
                TaskResult<GameObject> result = new();
                yield return CraftData.InstantiateFromPrefabAsync(techType, result);
                GameObject gameObject = result.Get();
                Pickupable pickupable = gameObject.GetComponent<Pickupable>();
                pickupable.Initialize();

                item.PickedUp(gameObject, techType);
                itemContainers.AddItem(gameObject, localPlayerId);
            }
        }
    }

    private static void SetPlayerStats(PlayerStatsData statsData)
    {
        if (statsData != null)
        {
            Player.main.oxygenMgr.AddOxygen(statsData.Oxygen);
            // Spawning a player with 0 health makes them invincible so we'd rather set it to 1 HP
            Player.main.liveMixin.health = Mathf.Max(1f, statsData.Health);
            Survival survivalComponent = Player.main.GetComponent<Survival>();
            survivalComponent.food = statsData.Food;
            survivalComponent.water = statsData.Water;
            Player.main.infectedMixin.SetInfectedAmount(statsData.InfectionAmount);

            //If InfectionAmount is at least 1f then the infection reveal should have happened already.
            //If InfectionAmount is below 1f then the reveal has not.
            if (statsData.InfectionAmount >= 1f)
            {
                Player.main.infectionRevealed = true;
            }

            // We need to make the player invincible before he finishes loading because in some cases he will eventually die before loading
            Player.main.liveMixin.invincible = true;
            Player.main.FreezeStats();
        }

        // We need to start it at least once for everything that's in the PDA to load
        Player.main.GetPDA().Open(PDATab.Inventory);
        Player.main.GetPDA().Close();
    }

    private static void SetPlayerGameMode(NitroxGameMode gameMode)
    {
        Log.Info($"Received initial sync packet with gamemode {gameMode}");
        GameModeUtils.SetGameMode((GameModeOption)(int)gameMode, GameModeOption.None);
    }

    private void ApplySettings(bool keepInventoryOnDeath, bool fastHatch, bool fastGrow)
    {
        localPlayer.KeepInventoryOnDeath = keepInventoryOnDeath;
        NoCostConsoleCommand.main.fastHatchCheat = fastHatch;
        NoCostConsoleCommand.main.fastGrowCheat = fastGrow;
        if (!fastHatch && !fastGrow)
        {
            return;
        }

        StringBuilder cheatsEnabled = new("Cheats enabled:");
        if (fastHatch)
        {
            cheatsEnabled.Append(" fastHatch");
        }
        if (fastGrow)
        {
            cheatsEnabled.Append(" fastGrow");
        }
        Log.InGame(cheatsEnabled.ToString());
    }
}
