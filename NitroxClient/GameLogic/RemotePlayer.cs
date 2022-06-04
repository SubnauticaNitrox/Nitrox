using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using UnityEngine;
using UWE;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic
{
    public class RemotePlayer : INitroxPlayer
    {
        private static readonly int animatorPlayerIn = Animator.StringToHash("player_in");

        private readonly PlayerModelManager playerModelManager;
        private readonly HashSet<TechType> equipment;

        public PlayerContext PlayerContext { get; }
        public GameObject Body { get; }
        public GameObject PlayerModel { get; }
        public Rigidbody RigidBody { get; }
        public ArmsController ArmsController { get; }
        public AnimationController AnimationController { get; }
        public ItemsContainer Inventory { get; }
        public Transform ItemAttachPoint { get; }

        public ushort PlayerId => PlayerContext.PlayerId;
        public string PlayerName => PlayerContext.PlayerName;
        public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;

        public Vehicle Vehicle { get; private set; }
        public SubRoot SubRoot { get; private set; }
        public EscapePod EscapePod { get; private set; }
        public PilotingChair PilotingChair { get; private set; }

        public readonly Event<RemotePlayer> PlayerDeathEvent = new();

        public readonly Event<RemotePlayer> PlayerDisconnectEvent = new();

        public RemotePlayer(GameObject playerBody, PlayerContext playerContext, List<TechType> equippedTechTypes, List<Pickupable> inventoryItems, PlayerModelManager modelManager)
        {
            PlayerContext = playerContext;

            Body = playerBody;
            Body.name = PlayerName;

            equipment = new HashSet<TechType>(equippedTechTypes);

            RigidBody = Body.AddComponent<Rigidbody>();
            RigidBody.useGravity = false;

            NitroxEntity.SetNewId(Body, playerContext.PlayerNitroxId);

            // Get player
            PlayerModel = Body.RequireGameObject("player_view");
            // Move variables to keep player animations from mirroring and for identification
            ArmsController = PlayerModel.GetComponent<ArmsController>();
            ArmsController.smoothSpeedUnderWater = 0;
            ArmsController.smoothSpeedAboveWater = 0;

            // ConditionRules has Player.Main based conditions from ArmsController
            PlayerModel.GetComponent<ConditionRules>().enabled = false;

            AnimationController = PlayerModel.AddComponent<AnimationController>();

            Transform inventoryTransform = new GameObject("Inventory").transform;
            inventoryTransform.SetParent(Body.transform);
            Inventory = new ItemsContainer(6, 8, inventoryTransform, "NitroxInventoryStorage_" + PlayerName, null);
            foreach (Pickupable item in inventoryItems)
            {
                Inventory.UnsafeAdd(new InventoryItem(item));
                Log.Debug($"Added {item.name} to {playerContext.PlayerName}.");
            }

            ItemAttachPoint = PlayerModel.transform.Find(PlayerEquipmentConstants.ITEM_ATTACH_POINT_GAME_OBJECT_NAME);

            playerModelManager = modelManager;
            playerModelManager.AttachPing(this);
            playerModelManager.BeginApplyPlayerColor(this);
            playerModelManager.RegisterEquipmentVisibilityHandler(PlayerModel);
            UpdateEquipmentVisibility();
            SetupSkyAppliers();

            ErrorMessage.AddMessage($"{PlayerName} joined the game.");
        }

        public void Attach(Transform transform, bool keepWorldTransform = false)
        {
            Body.transform.SetParent(transform);

            if (!keepWorldTransform)
            {
                UWE.Utils.ZeroTransform(Body);
            }
        }

        public void Detach()
        {
            Body.transform.SetParent(null);
        }

        public void UpdatePosition(Vector3 position, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation)
        {
            Body.SetActive(true);

            // When receiving movement packets, a player can not be controlling a vehicle (they can walk through subroots though).
            SetVehicle(null);
            SetPilotingChair(null);
            // If in a subroot the position will be relative to the subroot
            if (SubRoot && !SubRoot.isBase)
            {
                Quaternion vehicleAngle = SubRoot.transform.rotation;
                position = vehicleAngle * position;
                position += SubRoot.transform.position;
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

                if (PilotingChair)
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
                SkyEnvironmentChanged.Broadcast(Body, newSubRoot);
                if (newSubRoot)
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
                SkyEnvironmentChanged.Broadcast(Body, newEscapePod);
                if (newEscapePod)
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
                if (Vehicle)
                {
                    Vehicle.mainAnimator.SetBool(animatorPlayerIn, false);

                    Detach();
                    ArmsController.SetWorldIKTarget(null, null);
                    
                    Vehicle.GetComponent<MultiplayerVehicleControl>().Exit();
                }

                if (newVehicle)
                {
                    newVehicle.mainAnimator.SetBool(animatorPlayerIn, true);

                    Attach(newVehicle.playerPosition.transform);
                    ArmsController.SetWorldIKTarget(newVehicle.leftHandPlug, newVehicle.rightHandPlug);

                    // From here, a basic issue can happen.
                    // When a vehicle is docked since we joined a game and another player undocks him before the local player does, no MultiplayerVehicleControl can be found on the vehicle because they are only created when receiving VehicleMovement packets
                    // Therefore we need to make sure that the MultiplayerVehicleControl component exists before using it
                    switch (newVehicle)
                    {
                        case SeaMoth:
                            newVehicle.gameObject.EnsureComponent<MultiplayerSeaMoth>().Enter();
                            break;
                        case Exosuit:
                            newVehicle.gameObject.EnsureComponent<MultiplayerExosuit>().Enter();
                            break;
                    }
                }

                RigidBody.isKinematic = newVehicle;

                Vehicle = newVehicle;

                AnimationController["in_seamoth"] = newVehicle is SeaMoth;
                AnimationController["in_exosuit"] = AnimationController["using_mechsuit"] = newVehicle is Exosuit;
            }
        }

        public void Destroy()
        {
            ErrorMessage.AddMessage($"{PlayerName} left the game.");
            NitroxEntity.RemoveFrom(Body);
            Object.DestroyImmediate(Body);
        }

        public void UpdateAnimation(AnimChangeType type, AnimChangeState state)
        {
            switch (type)
            {
                case AnimChangeType.UNDERWATER:
                    AnimationController["is_underwater"] = state != AnimChangeState.OFF;
                    break;
                case AnimChangeType.BENCH:
                    AnimationController["cinematics_enabled"] = state != AnimChangeState.UNSET;
                    AnimationController["bench_sit"] = state == AnimChangeState.ON;
                    AnimationController["bench_stand_up"] = state == AnimChangeState.OFF;
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
            playerModelManager.UpdateEquipmentVisibility(new ReadOnlyCollection<TechType>(equipment.ToList()));
        }
        
        /// <summary>
        /// Allows the remote player model to have its lightings dynamicly adjusted
        /// </summary>
        private void SetupSkyAppliers()
        {
            // SkyAppliers apply the light effects of a lighting source on a set of renderers
            SkyApplier skyApplier = Body.AddComponent<SkyApplier>();
            skyApplier.anchorSky = Skies.Auto;
            skyApplier.emissiveFromPower = false;
            skyApplier.dynamic = true;
            skyApplier.renderers = Body.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }
    }
}
