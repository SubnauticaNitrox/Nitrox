using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayerJoinedMultiplayerSessionProcessor : ClientPacketProcessor<PlayerJoinedMultiplayerSession>
    {
        private readonly PlayerManager remotePlayerManager;

        public PlayerJoinedMultiplayerSessionProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(PlayerJoinedMultiplayerSession packet)
        {
            List<TechType> techTypes = packet.EquippedTechTypes.Select(techType => techType.ToUnity()).ToList();
            List<Pickupable> items = new List<Pickupable>();
            foreach (ItemData itemData in packet.InventoryItems)
            {
                GameObject item;
                try
                {
                    item = SerializationHelper.GetGameObject(itemData.SerializedData);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error deserializing item data. Id: {itemData.ItemId}");
                    continue;
                }

                Pickupable pickupable = item.GetComponent<Pickupable>().Initialize();
                pickupable.SetVisible(false);
                items.Add(pickupable);
            }

            remotePlayerManager.Create(packet.PlayerContext, packet.SubRootId, techTypes, items);
        }
    }
}
