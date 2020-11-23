using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Server;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class PlayerInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly ItemContainers itemContainers;
        private readonly IPacketSender packetSender;

        public PlayerInitialSyncProcessor(ItemContainers itemContainers, IPacketSender packetSender)
        {
            this.itemContainers = itemContainers;
            this.packetSender = packetSender;
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            SetPlayerGameObjectId(packet.PlayerGameObjectId);
            waitScreenItem.SetProgress(0.25f);
            yield return null;

            AddStartingItemsToPlayer(packet.FirstTimeConnecting);
            waitScreenItem.SetProgress(0.5f);
            yield return null;

            SetPlayerStats(packet.PlayerStatsData);
            waitScreenItem.SetProgress(0.75f);
            yield return null;

            SetPlayerGameMode(packet.GameMode);
            waitScreenItem.SetProgress(1f);
            yield return null;
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
                    itemContainers.AddItem(pickupable.gameObject, NitroxEntity.GetId(Player.main.transform.gameObject));
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
    }
}
