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
            MovementHelper.MoveGameObject(player, position, rotation);
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
            GameObject body = GameObject.Find("body");
            //Cheap fix for showing head, much easier since male_geo contains many different heads
            body.transform.parent.gameObject.GetComponent<Player>().head.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            GameObject bodyCopy = UnityEngine.Object.Instantiate(body);
            body.transform.parent.gameObject.GetComponent<Player>().head.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            bodyCopy.transform.Find("player_view").gameObject.GetComponent<ArmsController>().smoothSpeed = 0; //Disables the other character's move animations
            return bodyCopy;
        }
    }
}
