using System.Collections;
using System.Collections.Generic;
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
        private readonly ItemContainers itemContainers;

        public PlayerInitialSyncProcessor(ItemContainers itemContainers)
        {
            this.itemContainers = itemContainers;
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            SetPlayerPermissions(packet.Permissions);
            waitScreenItem.SetProgress(0.17f);
            yield return null;

            SetPlayerGameObjectId(packet.PlayerGameObjectId);
            waitScreenItem.SetProgress(0.33f);
            yield return null;

            AddStartingItemsToPlayer(packet.FirstTimeConnecting);
            waitScreenItem.SetProgress(0.5f);
            yield return null;

            SetPlayerStats(packet.PlayerStatsData);
            waitScreenItem.SetProgress(0.66f);
            yield return null;

            SetPlayerGameMode(packet.GameMode);
            waitScreenItem.SetProgress(0.83f);
            yield return null;

            SetPlayerCompletedGoals(packet.CompletedGoals);
            waitScreenItem.SetProgress(1f);
            yield return null;
        }

        private void SetPlayerPermissions(Perms permissions)
        {
            NitroxServiceLocator.LocateService<LocalPlayer>().Permissions = permissions;
        }

        private void SetPlayerGameObjectId(NitroxId id)
        {
            NitroxEntity.SetNewId(Player.mainObject, id);
            Log.Info($"Received initial sync player GameObject Id: {id}");
        }

        private void AddStartingItemsToPlayer(bool firstTimeConnecting)
        {
            if (firstTimeConnecting)
            {
                foreach (TechType techType in LootSpawner.main.GetEscapePodStorageTechTypes())
                {
                    GameObject gameObject = CraftData.InstantiateFromPrefab(techType, false);
                    Pickupable pickupable = gameObject.GetComponent<Pickupable>();
                    pickupable = pickupable.Initialize();
                    itemContainers.AddItem(pickupable.gameObject, NitroxEntity.GetId(Player.main.gameObject));
                    itemContainers.BroadcastItemAdd(pickupable, Inventory.main.container.tr);
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
            }
        }

        private void SetPlayerGameMode(ServerGameMode gameMode)
        {
            Log.Info($"Received initial sync packet with gamemode {gameMode}");
            GameModeUtils.SetGameMode((GameModeOption)(int)gameMode, GameModeOption.None);
        }

        private void SetPlayerCompletedGoals(ISet<string> completedGoals)
        {
            GoalManager.main.completedGoalNames.AddRange(completedGoals);
            PlayerWorldArrows.main.completedCustomGoals.AddRange(completedGoals);
        }
    }
}
