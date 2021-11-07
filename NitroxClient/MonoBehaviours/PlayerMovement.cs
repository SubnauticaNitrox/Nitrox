using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerMovement : MonoBehaviour
    {
        public const float BROADCAST_INTERVAL = 0.05f;
        private LocalPlayer localPlayer;

        private float time;
        public static bool ControllingCamera;
        public static MapRoomCamera ControlledCamera;
        private PacketSaver packetSaver;

        public void Awake()
        {
            localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
            packetSaver = new PacketSaver(localPlayer);
        }

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= BROADCAST_INTERVAL)
            {
                time = 0;

                Vector3 currentPosition = Player.main.transform.position;
                Vector3 playerVelocity = Player.main.playerController.velocity;

                // IDEA: possibly only CameraRotation is of interest, because bodyrotation is extracted from that.
                // WARN: Using camera rotation may be dangerous, when the drone is used for instance (but then movement packets shouldn't be sent either so it's not even relevant...)

                Quaternion bodyRotation = MainCameraControl.main.viewModel.transform.rotation;
                Quaternion aimingRotation = Player.main.camRoot.GetAimingTransform().rotation;

                Optional<VehicleMovementData> vehicle = GetVehicleMovement();
                SubRoot subRoot = Player.main.GetCurrentSub();
                // If in a subroot the position will be relative to the subroot
                if (subRoot != null)
                {
                    if (!subRoot.isBase)
                    {
                        // Rotate relative player position relative to the subroot (else there are problems with respawning)
                        Quaternion undoVehicleAngle = subRoot.transform.rotation.GetInverse();
                        currentPosition -= subRoot.transform.position;
                        currentPosition = undoVehicleAngle * currentPosition;
                        bodyRotation = undoVehicleAngle * bodyRotation;
                        aimingRotation = undoVehicleAngle * aimingRotation;
                    }
                    else if(ControlledCamera != null)
                    {
                        if(!ControllingCamera)
                        {
                            return;
                        }
                        NitroxVector3 position = ControlledCamera.transform.position.ToDto();
                        NitroxQuaternion rotation = ControlledCamera.transform.rotation.ToDto();
                        CameraMovementData cameraMovementData = new CameraMovementData(TechType.MapRoomCamera.ToDto(), NitroxEntity.GetId(ControlledCamera.gameObject), position, rotation);
                        if (packetSaver.SameCameraMovement(cameraMovementData))
                        {
                            return;
                        }
                        localPlayer.UpdateCameraLocation(cameraMovementData);
                        return;
                    }
                }

                if (vehicle.HasValue && packetSaver.SameVehicleMovement(vehicle.Value))
                {
                    // Please do something
                    // return;
                }
                if (packetSaver.SameMovement(currentPosition, playerVelocity, bodyRotation, aimingRotation))
                {
                    // Please do something
                    playerVelocity = Vector3.zero;
                    aimingRotation = Quaternion.identity;
                }
                localPlayer.UpdateLocation(currentPosition, playerVelocity, bodyRotation, aimingRotation, vehicle);
            }
        }

        private Optional<VehicleMovementData> GetVehicleMovement()
        {
            Vehicle vehicle = Player.main.GetVehicle();
            SubRoot sub = Player.main.GetCurrentSub();

            NitroxId id;
            Vector3 position;
            Quaternion rotation;
            Vector3 velocity;
            Vector3 angularVelocity;
            TechType techType;
            float steeringWheelYaw = 0f, steeringWheelPitch = 0f;
            bool appliedThrottle = false;
            Vector3 leftArmPosition = new Vector3(0, 0, 0);
            Vector3 rightArmPosition = new Vector3(0, 0, 0);

            if (vehicle != null)
            {
                id = NitroxEntity.GetId(vehicle.gameObject);
                position = vehicle.gameObject.transform.position;
                rotation = vehicle.gameObject.transform.rotation;
                techType = CraftData.GetTechType(vehicle.gameObject);

                Rigidbody rigidbody = vehicle.gameObject.GetComponent<Rigidbody>();

                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;

                // Required because vehicle is either a SeaMoth or an Exosuit, both types which can't see the fields either.
                steeringWheelYaw = vehicle.steeringWheelYaw;
                steeringWheelPitch = vehicle.steeringWheelPitch;

                // Vehicles (or the SeaMoth at least) do not have special throttle animations. Instead, these animations are always playing because the player can't even see them (unlike the cyclops which has cameras).
                // So, we need to hack in and try to figure out when thrust needs to be applied.
                if (vehicle && AvatarInputHandler.main.IsEnabled())
                {
                    if (techType == TechType.Seamoth)
                    {
                        bool flag = vehicle.transform.position.y < Ocean.main.GetOceanLevel() && vehicle.transform.position.y < vehicle.worldForces.waterDepth && !vehicle.precursorOutOfWater;
                        appliedThrottle = flag && GameInput.GetMoveDirection().sqrMagnitude > .1f;
                    }
                    else if (techType == TechType.Exosuit)
                    {
                        Exosuit exosuit = vehicle as Exosuit;
                        if (exosuit)
                        {
                            appliedThrottle = exosuit._jetsActive && exosuit.thrustPower > 0f;

                            Transform leftAim = exosuit.aimTargetLeft;
                            Transform rightAim = exosuit.aimTargetRight;

                            Vector3 eulerAngles = exosuit.transform.eulerAngles;
                            eulerAngles.x = MainCamera.camera.transform.eulerAngles.x;
                            Quaternion quaternion = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);

                            leftArmPosition = leftAim.transform.position = MainCamera.camera.transform.position + quaternion * Vector3.forward * 100f;
                            rightArmPosition = rightAim.transform.position = MainCamera.camera.transform.position + quaternion * Vector3.forward * 100f;
                        }
                    }
                }
            }
            else if (sub != null && Player.main.isPiloting)
            {
                if (sub.isCyclops)
                {
                    id = NitroxEntity.GetId(sub.gameObject);
                    position = sub.gameObject.transform.position;
                    rotation = sub.gameObject.transform.rotation;
                    Rigidbody rigidbody = sub.GetComponent<Rigidbody>();
                    velocity = rigidbody.velocity;
                    angularVelocity = rigidbody.angularVelocity;
                    techType = TechType.Cyclops;
                    SubControl subControl = sub.GetComponent<SubControl>();
                    steeringWheelYaw = subControl.steeringWheelYaw;
                    steeringWheelPitch = subControl.steeringWheelPitch;
                    appliedThrottle = subControl.appliedThrottle && subControl.canAccel;
                }
                // In the case of controlling a MapRoomCamera
                else
                {
                    return Optional.Empty;
                }
            }
            else
            {
                return Optional.Empty;
            }

            VehicleMovementData model = VehicleMovementFactory.GetVehicleMovementData(techType,
                                                                                        id,
                                                                                        position,
                                                                                        rotation,
                                                                                        velocity,
                                                                                        angularVelocity,
                                                                                        steeringWheelYaw,
                                                                                        steeringWheelPitch,
                                                                                        appliedThrottle,
                                                                                        leftArmPosition,
                                                                                        rightArmPosition);
            return Optional.Of(model);
        }

        public class PacketSaver
        {
            private LocalPlayer localPlayer;

            private CameraMovementData latestCameraMovementData;
            private VehicleMovementData latestVehicleMovementData;
            private Vector3 latestCurrentPosition;
            private Vector3 latestPlayerVelocity;
            private Quaternion latestBodyRotation;
            private Quaternion latestAimingRotation;

            private bool[] updatedLocationOnce;

            public PacketSaver(LocalPlayer localPlayer)
            {
                this.localPlayer = localPlayer;
                latestCurrentPosition = Vector3.zero;
                latestPlayerVelocity = Vector3.zero;
                latestBodyRotation = Quaternion.identity;
                latestAimingRotation = Quaternion.identity;
                updatedLocationOnce = new bool[] { false, false, false };
            }

            public bool SameCameraMovement(CameraMovementData cameraMovementData)
            {
                bool same = cameraMovementData.Equals(latestCameraMovementData);
                latestCameraMovementData = cameraMovementData;
                UpdateLocationOnce(same, 0);
                return same;
            }


            // Maybe SameMovement should just check currentPosition and bodyRotation ?
            public bool SameMovement(Vector3 currentPosition, Vector3 playerVelocity, Quaternion bodyRotation, Quaternion aimingRotation)
            {
                bool same = currentPosition.Equals(latestCurrentPosition)
                    && playerVelocity.Equals(latestPlayerVelocity)
                    && bodyRotation.Equals(latestBodyRotation)
                    && aimingRotation.Equals(latestAimingRotation);

                latestCurrentPosition = currentPosition;
                latestPlayerVelocity = playerVelocity;
                latestBodyRotation = bodyRotation;
                latestAimingRotation = aimingRotation;
                UpdateLocationOnce(same, 1);
                return same;
            }

            public bool SameVehicleMovement(VehicleMovementData vehicleMovementData)
            {
                bool same = vehicleMovementData.Equals(latestVehicleMovementData);
                latestVehicleMovementData = vehicleMovementData;
                UpdateLocationOnce(same, 2);
                return same;
            }

            public void UpdateLocationOnce(bool same, int movementType)
            {
                // TO BE FIXED
                return;
                if (!same)
                {
                    updatedLocationOnce[movementType] = false;
                }
                else if (!updatedLocationOnce[movementType])
                {
                    updatedLocationOnce[movementType] = true;
                    Vector3 currentPosition = Player.main.transform.position;
                    Quaternion bodyRotation = MainCameraControl.main.viewModel.transform.rotation;
                    localPlayer.UpdateLocation(currentPosition, Vector3.zero, bodyRotation, Quaternion.identity, Optional.Empty);
                }
            }
        }
    }
}
