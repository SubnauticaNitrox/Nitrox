using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic
{
    public class RemotePlayer
    {
        public readonly GameObject Body;
        public readonly GameObject PlayerView;
        public readonly AnimationController AnimationController;
        public readonly ArmsController ArmsController;
        public readonly Rigidbody RigidBody;

        public Vehicle Vehicle { get; private set; }
        public SubRoot SubRoot { get; private set; }
        public PilotingChair PilotingChair { get; private set; }

        public string PlayerId { get; }

        public RemotePlayer(string playerId)
        {
            PlayerId = playerId;
            GameObject originalBody = GameObject.Find("body");

            //Cheap fix for showing head, much easier since male_geo contains many different heads
            originalBody.GetComponentInParent<Player>().head.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            Body = Object.Instantiate(originalBody);
            originalBody.GetComponentInParent<Player>().head.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;


            RigidBody = Body.AddComponent<Rigidbody>();
            RigidBody.useGravity = false;

            //Get player
            PlayerView = Body.transform.Find("player_view").gameObject;

            //Move variables to keep player animations from mirroring and for identification
            ArmsController = PlayerView.GetComponent<ArmsController>();
            ArmsController.smoothSpeedUnderWater = 0;
            ArmsController.smoothSpeedAboveWater = 0;

            //Sets up a copy from the xSignal template for the signal
            //todo: settings menu to disable this?
            GameObject signalBase = Object.Instantiate(Resources.Load("VFX/xSignal")) as GameObject;
            signalBase.name = "signal" + playerId;
            signalBase.transform.localScale = new Vector3(.5f, .5f, .5f);
            signalBase.transform.localPosition += new Vector3(0, 0.8f, 0);
            signalBase.transform.SetParent(PlayerView.transform, false);
            PingInstance ping = signalBase.GetComponent<PingInstance>();
            ping.SetLabel("Player " + playerId);
            ping.pingType = PingType.Signal;

            AnimationController = PlayerView.AddComponent<AnimationController>();

            ErrorMessage.AddMessage($"{playerId} joined the game.");
        }

        public void Attach(Transform transform, bool keepWorldTransform = false)
        {
            Body.transform.parent = transform;

            if (!keepWorldTransform)
            {
                UWE.Utils.ZeroTransform(Body);
            }
        }

        public void Detach()
        {
            Body.transform.parent = null;
        }

        public void UpdatePosition(Vector3 position, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<string> opSubGuid)
        {
            Body.SetActive(true);

            SubRoot subRoot = null;
            if (opSubGuid.IsPresent())
            {
                GameObject sub = GuidHelper.RequireObjectFrom(opSubGuid.Get());
                subRoot = sub.GetComponent<SubRoot>();
            }

            // When receiving movement packets, a player can not be controlling a vehicle (they can walk through subroots though).
            SetVehicle(null);
            SetPilotingChair(null);
            SetSubRoot(subRoot);

            RigidBody.velocity = AnimationController.Velocity = MovementHelper.GetCorrectedVelocity(position, velocity, Body, PlayerMovement.BROADCAST_INTERVAL);
            RigidBody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(bodyRotation, Vector3.zero, Body, PlayerMovement.BROADCAST_INTERVAL);

            AnimationController.AimingRotation = aimingRotation;
            AnimationController.UpdatePlayerAnimations = true;
        }

        public void SetPilotingChair(PilotingChair newPilotingChair)
        {
            if (PilotingChair != newPilotingChair)
            {
                PilotingChair = newPilotingChair;

                Validate.NotNull(SubRoot, "Player changed PilotingChair but is not in SubRoot!");

                MultiplayerCyclops mpCyclops = SubRoot.GetComponent<MultiplayerCyclops>();

                if (PilotingChair != null)
                {
                    Attach(PilotingChair.sittingPosition.transform);
                    ArmsController.SetWorldIKTarget(PilotingChair.leftHandPlug, PilotingChair.rightHandPlug);

                    mpCyclops.CurrentPlayer = this;
                    mpCyclops.Enter();
                }
                else
                {
                    SetSubRoot(SubRoot);
                    ArmsController.SetWorldIKTarget(null, null);

                    mpCyclops.CurrentPlayer = null;
                    mpCyclops.Exit();
                }
                RigidBody.isKinematic = AnimationController["cyclops_steering"] = (newPilotingChair != null);
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
                    ArmsController.SetWorldIKTarget(null, null);

                    Vehicle.GetComponent<MultiplayerVehicleControl<Vehicle>>().Exit();
                }

                if (newVehicle != null)
                {
                    newVehicle.mainAnimator.SetBool("player_in", true);

                    Attach(newVehicle.playerPosition.transform);
                    ArmsController.SetWorldIKTarget(newVehicle.leftHandPlug, newVehicle.rightHandPlug);

                    newVehicle.GetComponent<MultiplayerVehicleControl<Vehicle>>().Enter();
                }

                RigidBody.isKinematic = newVehicle != null;

                Vehicle = newVehicle;

                AnimationController["in_seamoth"] = newVehicle is SeaMoth;
                AnimationController["in_exosuit"] = AnimationController["using_mechsuit"] = newVehicle is Exosuit;
            }
        }

        public void Destroy()
        {
            ErrorMessage.AddMessage($"{PlayerId} left the game.");
            Body.SetActive(false);
            Object.DestroyImmediate(Body);
        }

        public void UpdateAnimation(AnimChangeType type, AnimChangeState state)
        {
            switch (type)
            {
                case AnimChangeType.UNDERWATER:
                    AnimationController["is_underwater"] = state != AnimChangeState.OFF;
                    break;
            }
        }
    }
}
