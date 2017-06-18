using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerMovement : MonoBehaviour
    {
        private float time = 0.0f;
        public float interpolationPeriod = 0.25f;

        private Dictionary<String, GameObject> gameObjectByPlayerId = new Dictionary<String, GameObject>();
        
        public void Update()
        {
            recordMyPlayerMovement();
            playbackOtherPlayerMovements();
        }

        // Only do on a specific cadence to avoid hammering server
        public void recordMyPlayerMovement()
        {
            time += Time.deltaTime;

            if (time >= interpolationPeriod)
            {
                interpolationPeriod = 0;

                Vector3 currentPosition = Player.main.transform.position;
                Multiplayer.client.updatePlayerLocation(currentPosition);                
            }
        }

        private void playbackOtherPlayerMovements()
        {
            var movements = Multiplayer.client.getOtherPlayerMovements();
            while (movements.Count > 0)
            {
                Movement movement = movements.Dequeue();

                if(!gameObjectByPlayerId.ContainsKey(movement.PlayerId))
                {
                    gameObjectByPlayerId[movement.PlayerId] = createOtherPlayer(movement.PlayerId);
                }

                gameObjectByPlayerId[movement.PlayerId].transform.position = ApiHelper.Vector3(movement.PlayerPosition);
            }
        }

        private GameObject createOtherPlayer(String playerId)
        {
            return GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
    }
}
