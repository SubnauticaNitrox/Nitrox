using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxClient.GameLogic.Helper;
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

            //Sets up a copy from the xSignal template for the signal
            //todo: settings menu to disable this?
            GameObject signalBase = Object.Instantiate(Resources.Load("VFX/xSignal")) as GameObject;
            signalBase.name = "signal" + playerId;
            signalBase.transform.localScale = new Vector3(.5f, .5f, .5f);
            signalBase.transform.localPosition += new Vector3(0, 0.8f, 0);
            signalBase.transform.SetParent(playerView.transform, false);
            PingInstance ping = signalBase.GetComponent<PingInstance>();
            ping.SetLabel("Player " + playerId);
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
                var sub = GuidHelper.RequireObjectFrom(opSubGuid.Get());
                subRoot = sub.GetComponent<SubRoot>();
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

                Validate.NotNull(SubRoot, "Player changed PilotingChair but is not in SubRoot!");

                var mpCyclops = SubRoot.GetComponent<MultiplayerCyclops>();

                if (PilotingChair != null)
                {
                    Attach(PilotingChair.sittingPosition.transform);
                    armsController.SetWorldIKTarget(PilotingChair.leftHandPlug, PilotingChair.rightHandPlug);

                    mpCyclops.CurrentPlayer = this;
                    mpCyclops.Enter();
                }
                else
                {
                    SetSubRoot(SubRoot);
                    armsController.SetWorldIKTarget(null, null);

                    mpCyclops.CurrentPlayer = null;
                    mpCyclops.Exit();
                }
                rigidBody.isKinematic = animationController["cyclops_steering"] = (newPilotingChair != null);
            }
        }

        public void SetSubRoot(SubRoot newSubRoot)
        {
            if (SubRoot != newSubRoot)
            {
                if (newSubRoot != null)
                {
                    Attach(newSubRoot.transform, true);
                }
                else
                {
                    Detach();
                }

                SubRoot = newSubRoot;
            }
        }

        public void SetVehicle(Vehicle newVehicle)
        {
            if (Vehicle != newVehicle)
            {
                if (Vehicle != null)
                {
                    Vehicle.mainAnimator.SetBool("player_in", false);

                    Detach();
                    armsController.SetWorldIKTarget(null, null);

                    Vehicle.GetComponent<MultiplayerVehicleControl<Vehicle>>().Exit();
                }

                if (newVehicle != null)
                {
                    newVehicle.mainAnimator.SetBool("player_in", true);

                    Attach(newVehicle.playerPosition.transform);
                    armsController.SetWorldIKTarget(newVehicle.leftHandPlug, newVehicle.rightHandPlug);

                    newVehicle.GetComponent<MultiplayerVehicleControl<Vehicle>>().Enter();
                }

                rigidBody.isKinematic = newVehicle != null;

                Vehicle = newVehicle;

                animationController["in_seamoth"] = newVehicle is SeaMoth;
                animationController["in_exosuit"] = animationController["using_mechsuit"] = newVehicle is Exosuit;
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
