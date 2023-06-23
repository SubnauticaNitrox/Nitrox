using System.Collections;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Server;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class PlayerInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly Items item;
        private readonly ItemContainers itemContainers;

        public PlayerInitialSyncProcessor(Items item, ItemContainers itemContainers)
        {
            this.item = item;
            this.itemContainers = itemContainers;
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();

            SetPlayerPermissions(localPlayer, packet.Permissions);
            waitScreenItem.SetProgress(0.14f);
            yield return null;

            SetPlayerIntroCinematicMode(localPlayer, packet.IntroCinematicMode);
            waitScreenItem.SetProgress(0.28f);
            yield return null;

            SetPlayerGameObjectId(packet.PlayerGameObjectId);
            waitScreenItem.SetProgress(0.42f);
            yield return null;

            yield return AddStartingItemsToPlayer(packet.FirstTimeConnecting);
            waitScreenItem.SetProgress(0.56f);
            yield return null;

            SetPlayerStats(packet.PlayerStatsData);
            waitScreenItem.SetProgress(0.7f);
            yield return null;

            SetPlayerGameMode(packet.GameMode);
            waitScreenItem.SetProgress(0.84f);
            yield return null;
        }

        private void SetPlayerPermissions(LocalPlayer localPlayer, Perms permissions)
        {
            localPlayer.Permissions = permissions;
        }

        private void SetPlayerIntroCinematicMode(LocalPlayer localPlayer, IntroCinematicMode introCinematicMode)
        {
            localPlayer.IntroCinematicMode = introCinematicMode;
            Log.Info($"Received initial sync player IntroCinematicMode: {introCinematicMode}");
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
                    TaskResult<GameObject> result = new TaskResult<GameObject>();
                    yield return CraftData.InstantiateFromPrefabAsync(techType, result, false);
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
                Player.main.GetComponent<Survival>().food = statsData.Food;
                Player.main.GetComponent<Survival>().water = statsData.Water;
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

        private void SetPlayerGameMode(ServerGameMode gameMode)
        {
            Log.Info($"Received initial sync packet with gamemode {gameMode}");
            GameModeUtils.SetGameMode((GameModeOption)(int)gameMode, GameModeOption.None);
        }
    }
}
