using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class PlayerGameObjectManager
    {
        private Dictionary<string, GameObject> gameObjectByPlayerId = new Dictionary<string, GameObject>();

        public void UpdatePlayerPosition(string playerId, Vector3 position, Quaternion rotation)
        {
            GameObject player = GetPlayerGameObject(playerId);
            player.SetActive(true);
            player.transform.position = position;
            player.transform.rotation = rotation;
        }

        public void HidePlayerGameObject(string playerId)
        {
            GameObject player = GetPlayerGameObject(playerId);
            player.SetActive(false);
        }

        public GameObject FindPlayerGameObject(string playerId)
        {
            return gameObjectByPlayerId.GetOrDefault(playerId, null);
        }

        public GameObject GetPlayerGameObject(string playerId)
        {
            GameObject player = FindPlayerGameObject(playerId);
            if (player == null)
            {
                player = gameObjectByPlayerId[playerId] = createOtherPlayer(playerId);
            }
            return player;
        }

        private GameObject createOtherPlayer(string playerId)
        {
            return GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
    }
}
