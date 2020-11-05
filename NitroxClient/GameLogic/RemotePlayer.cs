using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NitroxClient.GameLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic
{
    public class RemotePlayer : INitroxPlayer
    {
        private readonly PlayerModelManager playerModelManager;
        private readonly HashSet<TechType> equipment;

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
        public EscapePod EscapePod { get; private set; }
        public PilotingChair PilotingChair { get; private set; }

        public RemotePlayer(GameObject playerBody, PlayerContext playerContext, List<TechType> equippedTechTypes, PlayerModelManager playerModelManager)
        {
            Body = playerBody;
            PlayerContext = playerContext;
            equipment = new HashSet<TechType>(equippedTechTypes);

            this.playerModelManager = playerModelManager;

            Body.name = PlayerName;

            RigidBody = Body.AddComponent<Rigidbody>();
            RigidBody.useGravity = false;

            // Get player
            PlayerModel = Body.RequireGameObject("player_view");

            // Move variables to keep player animations from mirroring and for identification
            ArmsController = PlayerModel.GetComponent<ArmsController>();
            ArmsController.smoothSpeedUnderWater = 0;
            ArmsController.smoothSpeedAboveWater = 0;

            AnimationController = PlayerModel.AddComponent<AnimationController>();

            playerModelManager.AttachPing(this);
            playerModelManager.BeginApplyPlayerColor(this);

            UpdateEquipmentVisibility();

            ErrorMessage.AddMessage($"{PlayerName} joined the game.");
        }

        public void ResetModel(ILocalNitroxPlayer localPlayer)
        {
            Body = Object.Instantiate(localPlayer.BodyPrototype);
            Body.SetActive(true);
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
            // If in a subroot the position will be relative to the subroot
            if (SubRoot != null && !SubRoot.isBase)
            {
                Quaternion vehicleAngle = SubRoot.transform.rotation;
                position = vehicleAngle * position;
                position = position + SubRoot.transform.position;
                bodyRotation = vehicleAngle * bodyRotation;
                aimingRotation = vehicleAngle * aimingRotation;
            }
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

        public void SetEscapePod(EscapePod newEscapePod)
        {
            if (EscapePod != newEscapePod)
            {
                if (newEscapePod != null)
                {
                    Attach(newEscapePod.transform, true);
                }
                else
                {
                    Detach();
                }

                EscapePod = newEscapePod;
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

        public void AddEquipment(TechType techType)
        {
            if (equipment.Contains(techType))
            {
                return;
            }

            equipment.Add(techType);

            UpdateEquipmentVisibility();
        }

        public void RemoveEquipment(TechType techType)
        {
            equipment.Remove(techType);
            UpdateEquipmentVisibility();
        }

        private void UpdateEquipmentVisibility()
        {
            ReadOnlyCollection<TechType> currentEquipment = new ReadOnlyCollection<TechType>(equipment.ToList());

            playerModelManager.UpdateEquipmentVisibility(PlayerModel, currentEquipment);
        }
    }
}
