using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class RemotePlayer : INitroxPlayer
    {
        public PlayerContext PlayerContext { get; }
        public GameObject Body { get; set; }
        public GameObject PlayerModel { get; set; }
        public Rigidbody RigidBody { get; }
        public ArmsController ArmsController { get; }
        public AnimationController AnimationController { get; }

        public ushort PlayerId => PlayerContext.PlayerId;
        public string PlayerName => PlayerContext.PlayerName;
        public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;

        public Vehicle Vehicle { get; private set; }
        public SubRoot SubRoot { get; private set; }
        public PilotingChair PilotingChair { get; private set; }

        public RemotePlayer(GameObject playerBody, PlayerContext playerContext)
        {
            Body = playerBody;
            PlayerContext = playerContext;

            Body.name = PlayerName;

            RigidBody = Body.AddComponent<Rigidbody>();
            RigidBody.useGravity = false;

            // Get player
            PlayerModel = Body.RequireGameObject("player_view");

            EquipRadiationSuit();

            // Move variables to keep player animations from mirroring and for identification
            ArmsController = PlayerModel.GetComponent<ArmsController>();
            ArmsController.smoothSpeedUnderWater = 0;
            ArmsController.smoothSpeedAboveWater = 0;

            AnimationController = PlayerModel.AddComponent<AnimationController>();

            ErrorMessage.AddMessage($"{PlayerName} joined the game.");
        }

        public void ResetModel(ILocalNitroxPlayer localPlayer)
        {
            Body = Object.Instantiate(localPlayer.BodyPrototype);
            PlayerModel = Body.RequireGameObject("player_view");
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

        public void UpdatePosition(Vector3 position, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation)
        {
            Body.SetActive(true);

            // When receiving movement packets, a player can not be controlling a vehicle (they can walk through subroots though).
            SetVehicle(null);
            SetPilotingChair(null);

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

                RigidBody.isKinematic = AnimationController["cyclops_steering"] = newPilotingChair != null;
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
            ErrorMessage.AddMessage($"{PlayerName} left the game.");
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

        private void EquipRadiationSuit()
        {
            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_body_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/diveSuit/diveSuit_body_geo").gameObject.SetActive(false);

            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_head_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/diveSuit/diveSuit_head_geo").gameObject.SetActive(false);

            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_gloves_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/diveSuit/diveSuit_hands_geo").gameObject.SetActive(false);

            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo/radiationSuit_vest_neckExtension_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo/radiationSuit_vest_neckExtension_geo/radiationSuit_helmet_geo 1").gameObject.SetActive(true);

            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo/radiationSuit_tank_geo 1").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo/radiationSuit_tank_geo 1/radiationSuit_tubes_geo 1").gameObject.SetActive(true);

            PlayerModel.transform.Find("male_geo/generalSuit").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/generalSuit/flippers_basic_geo").gameObject.SetActive(true);
        }

        private void EquipReinforcedSuit()
        {
            PlayerModel.transform.Find("male_geo/reinforcedSuit/reinforced_suit_01_body_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/diveSuit/diveSuit_body_geo").gameObject.SetActive(false);

            PlayerModel.transform.Find("male_geo/reinforcedSuit/reinforced_suit_01_glove_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/diveSuit/diveSuit_hands_geo").gameObject.SetActive(false);

            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_head_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/diveSuit/diveSuit_head_geo").gameObject.SetActive(false);

            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo/radiationSuit_vest_neckExtension_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo/radiationSuit_vest_neckExtension_geo/radiationSuit_helmet_geo 1").gameObject.SetActive(true);

            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo/radiationSuit_tank_geo 1").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/radiationSuit/radiationSuit_vest_reducedNeck_geo/radiationSuit_tank_geo 1/radiationSuit_tubes_geo 1").gameObject.SetActive(true);

            PlayerModel.transform.Find("male_geo/SwimChargeFins").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/SwimChargeFins/flippers_basic_geo").gameObject.SetActive(true);
        }

        private void EquipStillSuit()
        {
            PlayerModel.transform.Find("male_geo/stillSuit/still_suit_01_body_geo").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/diveSuit/diveSuit_body_geo").gameObject.SetActive(false);

            PlayerModel.transform.Find("male_geo/scubaSuit").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/scubaSuit/scuba_head").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/diveSuit/diveSuit_head_geo").gameObject.SetActive(false);

            PlayerModel.transform.Find("male_geo/scubaSuit/scuba_vest").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/scubaSuit/scuba_vest/scuba_breatherTubes").gameObject.SetActive(true);

            PlayerModel.transform.Find("male_geo/UltraGlideFins").gameObject.SetActive(true);
            PlayerModel.transform.Find("male_geo/UltraGlideFins/flippers_basic_geo").gameObject.SetActive(true);
        }
    }
}
