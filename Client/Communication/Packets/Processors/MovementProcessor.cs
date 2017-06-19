using NitroxClient.Communication.Packets.Processors.Base;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MovementProcessor : GenericPacketProcessor<Movement>
    {
        private Dictionary<String, GameObject> gameObjectByPlayerId = new Dictionary<String, GameObject>();

        public override void Process(Movement movement)
        {
            if (!gameObjectByPlayerId.ContainsKey(movement.PlayerId))
            {
                gameObjectByPlayerId[movement.PlayerId] = createOtherPlayer(movement.PlayerId);
            }

            gameObjectByPlayerId[movement.PlayerId].transform.position = ApiHelper.Vector3(movement.PlayerPosition);
        }

        private GameObject createOtherPlayer(String playerId)
        {
            return GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
    }
}
