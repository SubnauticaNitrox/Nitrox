using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.Communication
{
    public class MultiplayerClient
    {
        private String playerId;
        private TcpClient client;
        public NitroxModel.DataStructures.Vector3 MockedPlayerPosition { get; set; }
        
        public MultiplayerClient(String playerId)
        {
            this.playerId = playerId;
            FileLogger.LogInfo("Starting Multiplayer for player " + playerId);
            connect();
            authenticate();
        }

        private void connect()
        {
            try
            {
                client = new TcpClient();
                client.Start();
                FileLogger.LogInfo("Connected to server successfully");
            }
            catch (Exception ex)
            {
                FileLogger.LogError("Could not start server", ex);
            }
        }

        private void authenticate()
        {
            try
            {
                Authenticate auth = new Authenticate(playerId);
                client.Send(auth);
                FileLogger.LogInfo("Sent auth packet to server");
            }
            catch (Exception ex)
            {
                FileLogger.LogError("Could not send auth packet", ex);
            }
        }

        public void updatePlayerLocation(Vector3 location)
        {
            try
            {
                Movement move = new Movement(playerId, ApiHelper.Vector3(location));
                client.Send(move);
            }
            catch (Exception ex)
            {
                FileLogger.LogError("Error sending move command", ex);
            }
        }

        public void pickupItem(Vector3 itemPosition, String gameObjectName, String techType)
        {
            try
            {
                PickupItem pickupItem = new PickupItem(playerId, getPlayerPosition(), ApiHelper.Vector3(itemPosition), gameObjectName, techType);
                client.Send(pickupItem);
                FileLogger.LogInfo("Sent " + playerId + " picked up " + techType + " " + gameObjectName + " at " + itemPosition);
            }
            catch (Exception ex)
            {
                FileLogger.LogError("Error sending pickup command", ex);
            }
        }

        public void dropItem(String techType, Vector3 itemPosition, Vector3 pushVelocity)
        {
            try
            {
                DroppedItem droppedItem = new DroppedItem(playerId, techType, getPlayerPosition(), ApiHelper.Vector3(itemPosition), ApiHelper.Vector3(pushVelocity));
                client.Send(droppedItem);
                FileLogger.LogInfo("Sent " + playerId + " dropped up " + techType + " at " + itemPosition + " with velocity " + pushVelocity);
            }
            catch (Exception ex)
            {
                FileLogger.LogError("Error sending drop command", ex);
            }
        }

        public void buildItem(String techType, Vector3 itemPosition, Quaternion quaternion)
        {
            try
            {
                BeginItemConstruction buildItem = new BeginItemConstruction(playerId, getPlayerPosition(), ApiHelper.Vector3(itemPosition), ApiHelper.Quaternion(quaternion), techType);
                client.Send(buildItem);
                FileLogger.LogInfo("Sent " + playerId + " dropped up " + techType + " at " + itemPosition + " with quaternion " + quaternion);
            }
            catch (Exception ex)
            {
                FileLogger.LogError("Error sending drop command", ex);
            }
        }

        public void changeConstructionAmount(Vector3 itemPosition, float amount)
        {
            try
            {
                ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(playerId, getPlayerPosition(), ApiHelper.Vector3(itemPosition), amount);
                client.Send(amountChanged);
                FileLogger.LogInfo("Sent " + playerId + " changed construction amount at " + itemPosition + " with amount " + amount);
            }
            catch (Exception ex)
            {
                FileLogger.LogError("Error sending drop command", ex);
            }
        }

        public Queue<Movement> getOtherPlayerMovements()
        {
            Queue<Movement> moves = client.getReceivedPacketsOfType<Movement>();
            //FileLogger.LogInfo("returning " + moves.Count() + " moves");
            return moves;
        }

        public Queue<PickupItem> getPickedUpItems()
        {
            Queue<PickupItem> pickedUpItems = client.getReceivedPacketsOfType<PickupItem>();
            //FileLogger.LogInfo("returning " + pickedUpItems.Count() + " picked up items");
            return pickedUpItems;
        }

        public Queue<DroppedItem> getDroppedItems()
        {
            Queue<DroppedItem> droppedItems = client.getReceivedPacketsOfType<DroppedItem>();
            //FileLogger.LogInfo("returning " + droppedItems.Count() + " dropped up items");
            return droppedItems;
        }

        public Queue<BeginItemConstruction> getBeginningItemConstructions()
        {
            Queue<BeginItemConstruction> constructions = client.getReceivedPacketsOfType<BeginItemConstruction>();
            //FileLogger.LogInfo("returning " + builtItem.Count() + " built items");
            return constructions;
        }

        public Queue<ConstructionAmountChanged> getConstrutionAmountChanged()
        {
            Queue<ConstructionAmountChanged> constructions = client.getReceivedPacketsOfType<ConstructionAmountChanged>();
            //FileLogger.LogInfo("returning " + builtItem.Count() + " built items");
            return constructions;
        }

        public NitroxModel.DataStructures.Vector3 getPlayerPosition()
        {
#if (DEBUG)
            return MockedPlayerPosition;
#else
            return ApiHelper.Vector3(Player.main.gameObject.transform.position);
#endif            
        }

    }
}
