using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
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

        public override void Process(InitialPlayerSync packet)
        {
            SetPlayerGuid(packet.PlayerGuid);
            AddStartingItemsToPlayer(packet.FirstTimeConnecting);
            SetPlayerStats(packet.PlayerStatsData);
            SetPlayerGameMode((GameModeOption)packet.GameMode);
        }

        private void SetPlayerGuid(string playerguid)
        {
            GuidHelper.SetNewGuid(Player.mainObject, playerguid);
            Log.Info("Received initial sync Player Guid: " + playerguid);
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
                    itemContainers.AddItem(pickupable.gameObject, GuidHelper.GetGuid(Player.main.transform.gameObject));
                    itemContainers.BroadcastItemAdd(pickupable, Inventory.main.container.tr);
                }
            }
        }

        private void SetPlayerStats(PlayerStatsData statsData)
        {
            if (statsData != null)
            {
                using (packetSender.Suppress<PlayerStats>())
                {
                    Player.main.oxygenMgr.AddOxygen(statsData.Oxygen);
                }
            }
        }
        
        private void SetPlayerGameMode(GameModeOption gameMode)
        {
            Log.Info("Recieved initial sync packet with game mode " + gameMode);
            GameModeUtils.SetGameMode(gameMode, GameModeOption.None);
        }
    }
}
