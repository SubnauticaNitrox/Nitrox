using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class PlayerGameObjectManager
    {
        private Dictionary<String, GameObject> gameObjectByPlayerId = new Dictionary<String, GameObject>();
        
        public void UpdatePlayerPosition(String playerId, Vector3 position, Quaternion rotation)
        {
            GameObject player = GetPlayerGameObject(playerId);
            player.SetActive(true);
            player.transform.position = position;
            player.transform.rotation = rotation;
        }

        public void HidePlayerGameObject(String playerId)
        {
            GameObject player = GetPlayerGameObject(playerId);
            player.SetActive(false);
        }

        public GameObject GetPlayerGameObject(String playerId)
        {
            if (!gameObjectByPlayerId.ContainsKey(playerId))
            {
                gameObjectByPlayerId[playerId] = createOtherPlayer(playerId);
            }

            return gameObjectByPlayerId[playerId];
        }

        private GameObject createOtherPlayer(String playerId)
        {
            return GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
    }
}
