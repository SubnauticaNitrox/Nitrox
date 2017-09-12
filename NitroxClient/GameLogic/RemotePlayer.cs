using LitJson;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
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
        public readonly ArmsController armsController;
        public readonly Rigidbody rigidBody;

        public Vehicle Vehicle { get; private set; }
        public SubRoot SubRoot { get; private set; }
        public PilotingChair PilotingChair { get; private set; }

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
            rigidBody.useGravity = false;

            //Get player
            playerView = body.transform.Find("player_view").gameObject;

            //Move variables to keep player animations from mirroring and for identification
            armsController = playerView.GetComponent<ArmsController>();
            armsController.smoothSpeed = 0;

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

        public void Attach(Transform transform, bool keepWorldTransform = false)
        {
            body.transform.parent = transform;

            if (!keepWorldTransform)
            {
                UWE.Utils.ZeroTransform(body);
            }
        }

        public void Detach()
        {
            body.transform.parent = null;
        }

        public void UpdatePosition(Vector3 position, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<string> opSubGuid)
        {
            body.SetActive(true);

            SubRoot subRoot = null;
            if (opSubGuid.IsPresent())
            {
                string subGuid = opSubGuid.Get();
                Optional<GameObject> opSub = GuidHelper.GetObjectFrom(subGuid);

                if (opSub.IsPresent())
                {
                    subRoot = opSub.Get().GetComponent<SubRoot>();
                }
                else
                {
                    Console.WriteLine("Could not find sub for guid: " + subGuid);
                }
            }

            // When receiving movement packets, a player can not be controlling a vehicle (they can walk through subroots though).
            SetVehicle(null);
            SetPilotingChair(null);
            SetSubRoot(subRoot);

            rigidBody.velocity = animationController.Velocity = MovementHelper.GetCorrectedVelocity(position, velocity, body, PlayerMovement.BROADCAST_INTERVAL);
            rigidBody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(bodyRotation, Vector3.zero, body, PlayerMovement.BROADCAST_INTERVAL);

            animationController.AimingRotation = aimingRotation;
            animationController.UpdatePlayerAnimations = true;
        }

        public void SetPilotingChair(PilotingChair newPilotingChair)
        {
            if (PilotingChair != newPilotingChair)
            {
                PilotingChair = newPilotingChair;

                if (PilotingChair != null)
                {
                    Attach(PilotingChair.sittingPosition.transform);
                    armsController.SetWorldIKTarget(PilotingChair.leftHandPlug, PilotingChair.rightHandPlug);
                }
                else
                {
                    Validate.NotNull(SubRoot, "Player left PilotingChair but is not in SubRoot!");
                    SetSubRoot(SubRoot);
                    armsController.SetWorldIKTarget(null, null);
                }
                rigidBody.isKinematic = animationController["cyclops_steering"] = (newPilotingChair != null);
            }
        }

        public void SetSubRoot(SubRoot newSubRoot)
        {
            if (SubRoot != newSubRoot)
            {
                Console.WriteLine("Next subroot: {0}");
                var existing = newSubRoot ?? SubRoot;
                if (existing)
                {
                    var mpCyclops = existing.GetComponent<MultiplayerCyclops>();
                    if (mpCyclops != null)
                    {
                        mpCyclops.CurrentPlayer = newSubRoot == null ? null : this;
                    }
                }

                SubRoot = newSubRoot;

                if (SubRoot != null)
                {
                    Attach(SubRoot.transform, true);
                }
                else
                {
                    Detach();
                }
            }
        }

        public void SetVehicle(Vehicle newVehicle)
        {
            if (Vehicle != newVehicle)
            {
                var existing = newVehicle ?? Vehicle;
                existing?.mainAnimator.SetBool("player_in", newVehicle != null);

                var existingSeamoth = existing as SeaMoth;
                var existingExosuit = existing as Exosuit;

                Vehicle = newVehicle;

                rigidBody.isKinematic = (Vehicle != null);

                if (Vehicle != null)
                {
                    Attach(Vehicle.playerPosition.transform);
                    armsController.SetWorldIKTarget(Vehicle.leftHandPlug, Vehicle.rightHandPlug);
                }
                else
                {
                    Detach();
                    armsController.SetWorldIKTarget(null, null);
                    existingSeamoth?.bubbles.Stop();
                }
                animationController["in_seamoth"] = Vehicle is SeaMoth;
                animationController["in_exosuit"] = animationController["using_mechsuit"] = Vehicle is Exosuit;
            }
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
                    animationController["is_underwater"] = state != AnimChangeState.Off;
                    break;
            }
        }
    }
}
