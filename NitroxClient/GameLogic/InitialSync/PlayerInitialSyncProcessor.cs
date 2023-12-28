using System.Collections;
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
public class PlayerInitialSyncProcessor : InitialSyncProcessor
{
    private readonly Items item;
    private readonly ItemContainers itemContainers;
    private readonly LocalPlayer localPlayer;

    public PlayerInitialSyncProcessor(Items item, ItemContainers itemContainers, LocalPlayer localPlayer)
    {
        this.item = item;
        this.itemContainers = itemContainers;
        this.localPlayer = localPlayer;

        AddStep(sync => SetPlayerPermissions(sync.Permissions));
        AddStep(sync => SetPlayerGameObjectId(sync.PlayerGameObjectId));
        AddStep(sync => AddStartingItemsToPlayer(sync.FirstTimeConnecting));
        AddStep(sync => SetPlayerStats(sync.PlayerStatsData));
        AddStep(sync => SetPlayerGameMode(sync.GameMode));
    }

    private void SetPlayerPermissions(Perms permissions)
    {
        localPlayer.Permissions = permissions;
    }

    private void SetPlayerGameObjectId(NitroxId id)
    {
        NitroxEntity.SetNewId(Player.mainObject, id);
        Log.Info($"Received initial sync player GameObject Id: {id}");
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

                item.Created(gameObject);
                itemContainers.AddItem(gameObject, localPlayerId);
            }
        }
    }

    private void SetPlayerStats(PlayerStatsData statsData)
    {
        if (statsData != null)
        {
            Player.main.oxygenMgr.AddOxygen(statsData.Oxygen);
            Player.main.liveMixin.health = statsData.Health;
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

    private void SetPlayerGameMode(NitroxGameMode gameMode)
    {
        Log.Info($"Received initial sync packet with gamemode {gameMode}");
        GameModeUtils.SetGameMode((GameModeOption)(int)gameMode, GameModeOption.None);
    }
}
