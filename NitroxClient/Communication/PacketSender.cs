using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures.ServerModel;
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
        public String PlayerId { get; set; }

        private TcpClient client;

        public PacketSender(TcpClient client)
        {
            this.client = client;
            this.Active = false;
        }

        public void Authenticate()
        {
            Authenticate auth = new Authenticate(PlayerId);
            Send(auth);
        }

        public void UpdatePlayerLocation(Vector3 location, Quaternion rotation, Optional<VehicleModel> opVehicle)
        {
            Movement movement;

            if (opVehicle.IsPresent())
            {
                VehicleModel vehicle = opVehicle.Get();
                movement = new VehicleMovement(PlayerId, ApiHelper.Vector3(location), vehicle.Rotation, vehicle.TechType, vehicle.Guid);
            }
            else
            {
                movement = new Movement(PlayerId, ApiHelper.Vector3(location), ApiHelper.Quaternion(rotation));
            }

            Send(movement);
        }
        
        public void PickupItem(Vector3 itemPosition, String gameObjectName, String techType)
        {
            PickupItem pickupItem = new PickupItem(PlayerId, ApiHelper.Vector3(itemPosition), gameObjectName, techType);
            Send(pickupItem);
        }

        public void DropItem(String techType, Vector3 itemPosition, Vector3 pushVelocity)
        {
            DroppedItem droppedItem = new DroppedItem(PlayerId, techType, ApiHelper.Vector3(itemPosition), ApiHelper.Vector3(pushVelocity));
            Send(droppedItem);
        }

        public void BuildItem(String techType, Vector3 itemPosition, Quaternion quaternion)
        {
            BeginItemConstruction buildItem = new BeginItemConstruction(PlayerId, ApiHelper.Vector3(itemPosition), ApiHelper.Quaternion(quaternion), techType);
            Send(buildItem);
        }

        public void ChangeConstructionAmount(Vector3 itemPosition, float amount, int resourceId1, int resourceId2)
        {
            if (amount >= 1f || resourceId1 != resourceId2)
            {
                ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(PlayerId, ApiHelper.Vector3(itemPosition), amount);
                Send(amountChanged);
            }
        }

        public void SendChatMessage(String text)
        {
            ChatMessage message = new ChatMessage(PlayerId, text);
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
