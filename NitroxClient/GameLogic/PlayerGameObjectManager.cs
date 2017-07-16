using LitJson;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class PlayerGameObjectManager
    {
        private const float PLAYER_TRANSFORM_SMOOTH_PERIOD = 0.05f;

        private Dictionary<string, GameObject> gameObjectByPlayerId = new Dictionary<string, GameObject>();

        public void UpdatePlayerPosition(string playerId, Vector3 position, Quaternion rotation, Optional<String> opSubGuid)
        {
            GameObject player = GetPlayerGameObject(playerId);
            player.SetActive(true);

            if(opSubGuid.IsPresent())
            {
                String subGuid = opSubGuid.Get();
                Optional<GameObject> opSub = GuidHelper.GetObjectFrom(subGuid);

                if(opSub.IsPresent())
                {
                    player.transform.parent = opSub.Get().transform;
                }
                else
                {
                    Console.WriteLine("Could not find sub for guid: " + subGuid);
                }
            }

            MovementHelper.MoveGameObject(player, position, rotation, PLAYER_TRANSFORM_SMOOTH_PERIOD);
        }

        public void UpdateAnimation(string playerId, AnimChangeType type, AnimChangeState state)
        {
            GameObject player = GetPlayerGameObject(playerId);
            GameObject playerView = player.transform.Find("player_view").gameObject;
            AnimationController controller = playerView.GetComponent<AnimationController>();

            bool animationValue;

            switch (type)
            {
                case AnimChangeType.Underwater:
                    animationValue = (state != AnimChangeState.Off);
                    controller.SetBool("is_underwater", animationValue);
                    break;
            }
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
                player = gameObjectByPlayerId[playerId] = CreateOtherPlayer(playerId);
            }
            return player;
        }

        private GameObject CreateOtherPlayer(string playerId)
        {
            GameObject body = GameObject.Find("body");
            //Cheap fix for showing head, much easier since male_geo contains many different heads
            body.transform.parent.gameObject.GetComponent<Player>().head.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            GameObject bodyCopy = UnityEngine.Object.Instantiate(body);
            body.transform.parent.gameObject.GetComponent<Player>().head.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

            //Get player
            GameObject playerView = bodyCopy.transform.Find("player_view").gameObject;
            //Move variables to keep player animations from mirroring and for identification
            playerView.GetComponent<ArmsController>().smoothSpeed = 0;

            //Sets a new language value
            Language language = Language.main;
            JsonData data = (JsonData)language.ReflectionGet("strings"); //UM4SN only: JsonData data = language.strings;
            data["Signal_" + playerId] = "Player " + playerId;

            //Sets up a copy from the xSignal template for the signal
            //todo: settings menu to disable this?
            GameObject signalBase = UnityEngine.Object.Instantiate(Resources.Load("VFX/xSignal")) as GameObject;
            signalBase.name = "signal" + playerId;
            signalBase.transform.localPosition += new Vector3(0, 0.8f, 0);
            signalBase.transform.SetParent(playerView.transform, false);
            SignalLabel label = signalBase.GetComponent<SignalLabel>();
            PingInstance ping = signalBase.GetComponent<PingInstance>();
            label.description = "Signal_" + playerId;
            ping.pingType = PingType.Signal;
            
            playerView.AddComponent<AnimationController>();
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
