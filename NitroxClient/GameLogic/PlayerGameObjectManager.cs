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
            MovementHelper.MoveGameObject(player, position, rotation);
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
            GameObject body = GameObject.Find("body");
            //Cheap fix for showing head, much easier since male_geo contains many different heads
            body.transform.parent.gameObject.GetComponent<Player>().head.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            GameObject bodyCopy = UnityEngine.Object.Instantiate(body);
            body.transform.parent.gameObject.GetComponent<Player>().head.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            bodyCopy.transform.Find("player_view").gameObject.GetComponent<ArmsController>().smoothSpeed = 0; //Disables the other character's move animations
            return bodyCopy;
        }

        public void RemovePlayerGameObject(String playerId)
        {
            GameObject player = gameObjectByPlayerId[playerId];
            player.SetActive(false);
            GameObject.DestroyImmediate(player);
            gameObjectByPlayerId.Remove(playerId);
        }
    }
}
