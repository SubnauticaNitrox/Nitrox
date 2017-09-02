using LitJson;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic
{
    public class RemotePlayer
    {
        public readonly GameObject body;
        public readonly GameObject playerView;
        public readonly AnimationController animationController;
        public readonly Rigidbody rigidBody;

        public string PlayerId { get; private set; }

        public RemotePlayer(string playerId)
        {
            PlayerId = playerId;
            GameObject originalBody = GameObject.Find("body");

            //Cheap fix for showing head, much easier since male_geo contains many different heads
            originalBody.GetComponentInParent<Player>().head.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            body = Object.Instantiate(originalBody);
            originalBody.GetComponentInParent<Player>().head.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

            rigidBody = body.AddComponent<Rigidbody>();

            //Get player
            playerView = body.transform.Find("player_view").gameObject;

            //Move variables to keep player animations from mirroring and for identification
            playerView.GetComponent<ArmsController>().smoothSpeed = 0;

            //Sets a new language value
            Language language = Language.main;
            JsonData data = (JsonData)language.ReflectionGet("strings"); //UM4SN only: JsonData data = language.strings;
            data["Signal_" + playerId] = "Player " + playerId;

            //Sets up a copy from the xSignal template for the signal
            //todo: settings menu to disable this?
            GameObject signalBase = Object.Instantiate(Resources.Load("VFX/xSignal")) as GameObject;
            signalBase.name = "signal" + playerId;
            signalBase.transform.localPosition += new Vector3(0, 0.8f, 0);
            signalBase.transform.SetParent(playerView.transform, false);
            SignalLabel label = signalBase.GetComponent<SignalLabel>();
            PingInstance ping = signalBase.GetComponent<PingInstance>();
            label.description = "Signal_" + playerId;
            ping.pingType = PingType.Signal;

            animationController = playerView.AddComponent<AnimationController>();

            ErrorMessage.AddMessage($"{playerId} joined the game.");
        }

        public void UpdatePosition(Vector3 position, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<string> opSubGuid)
        {
            body.SetActive(true);
            if (opSubGuid.IsPresent())
            {
                string subGuid = opSubGuid.Get();
                Optional<GameObject> opSub = GuidHelper.GetObjectFrom(subGuid);

                if (opSub.IsPresent())
                {
                    body.transform.parent = opSub.Get().transform;
                }
                else
                {
                    Console.WriteLine("Could not find sub for guid: " + subGuid);
                }
            }

            rigidBody.velocity = animationController.Velocity = MovementHelper.GetCorrectedVelocity(position, velocity, body, PlayerMovement.BROADCAST_INTERVAL);
            rigidBody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(bodyRotation, body, PlayerMovement.BROADCAST_INTERVAL);

            animationController.AimingRotation = aimingRotation;
            animationController.UpdatePlayerAnimations = true;
        }

        public void Destroy()
        {
            ErrorMessage.AddMessage($"{PlayerId} left the game.");
            body.SetActive(false);
            Object.DestroyImmediate(body);
        }

        public void UpdateAnimation(AnimChangeType type, AnimChangeState state)
        {
            switch (type)
            {
                case AnimChangeType.Underwater:
                    animationController.SetBool("is_underwater", state != AnimChangeState.Off);
                    break;
            }
        }
    }
}
