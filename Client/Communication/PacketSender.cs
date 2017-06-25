using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.Communication
{
    public class PacketSender
    {
        public bool Active { get; set; }

        private TcpClient client;
        private String playerId;

        public PacketSender(TcpClient client, String playerId)
        {
            this.client = client;
            this.playerId = playerId;
            this.Active = false;
        }

        public void Authenticate()
        {
            Authenticate auth = new Authenticate(playerId);
            Send(auth);
        }

        public void UpdatePlayerLocation(Vector3 location)
        {
            Movement move = new Movement(playerId, ApiHelper.Vector3(location));
            Send(move);
        }
        
        public void PickupItem(Vector3 itemPosition, String gameObjectName, String techType)
        {
            PickupItem pickupItem = new PickupItem(playerId, ApiHelper.Vector3(itemPosition), gameObjectName, techType);
            Send(pickupItem);
        }

        public void DropItem(String techType, Vector3 itemPosition, Vector3 pushVelocity)
        {
            DroppedItem droppedItem = new DroppedItem(playerId, techType, ApiHelper.Vector3(itemPosition), ApiHelper.Vector3(pushVelocity));
            Send(droppedItem);
        }

        public void BuildItem(String techType, Vector3 itemPosition, Quaternion quaternion)
        {
            BeginItemConstruction buildItem = new BeginItemConstruction(playerId, ApiHelper.Vector3(itemPosition), ApiHelper.Quaternion(quaternion), techType);
            Send(buildItem);
        }

        public void ChangeConstructionAmount(Vector3 itemPosition, float amount)
        {
            ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(playerId, ApiHelper.Vector3(itemPosition), amount);
            Send(amountChanged);
        }

        public void SendChatMessage(String text)
        {
            ChatMessage message = new ChatMessage(playerId, text);
            Send(message);
        }

        private void Send(Packet packet)
        {
            if (Active)
            {
                try
                {
                    client.Send(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending packet " + packet, ex);
                }
            }
        }
    }
}
