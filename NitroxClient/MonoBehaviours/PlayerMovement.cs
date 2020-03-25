using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerMovement : MonoBehaviour
    {
        public const float BROADCAST_INTERVAL = 0.05f;
        private LocalPlayer localPlayer;

        private float time;

        public void Awake()
        {
            localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
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
                if (subRoot != null && !subRoot.isBase)
                {
                    // Rotate relative player position relative to the subroot (else there are problems with respawning)
                    Quaternion undoVehicleAngle = subRoot.transform.rotation.GetInverse();
                    currentPosition = currentPosition - subRoot.transform.position;
                    currentPosition = undoVehicleAngle * currentPosition;
                    bodyRotation = undoVehicleAngle * bodyRotation;
                    aimingRotation = undoVehicleAngle * aimingRotation;
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
            float health;

            if (vehicle != null)
            {
                id = NitroxEntity.GetId(vehicle.gameObject);
                position = vehicle.gameObject.transform.position;
                rotation = vehicle.gameObject.transform.rotation;
                techType = CraftData.GetTechType(vehicle.gameObject);
                health = vehicle.GetComponent<LiveMixin>().health;

                Rigidbody rigidbody = vehicle.gameObject.GetComponent<Rigidbody>();

                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;

                // Required because vehicle is either a SeaMoth or an Exosuit, both types which can't see the fields either.
                steeringWheelYaw = (float)vehicle.ReflectionGet<Vehicle, Vehicle>("steeringWheelYaw");
                steeringWheelPitch = (float)vehicle.ReflectionGet<Vehicle, Vehicle>("steeringWheelPitch");

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
                            appliedThrottle = (bool)exosuit.ReflectionGet("_jetsActive") && (float)exosuit.ReflectionGet("thrustPower") > 0f;

                            Transform leftAim = (Transform)exosuit.ReflectionGet("aimTargetLeft", true);
                            Transform rightAim = (Transform)exosuit.ReflectionGet("aimTargetRight", true);

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
                id = NitroxEntity.GetId(sub.gameObject);
                position = sub.gameObject.transform.position;
                rotation = sub.gameObject.transform.rotation;
                health = sub.GetComponent<LiveMixin>().health;
                Rigidbody rigidbody = sub.GetComponent<Rigidbody>();
                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;
                techType = TechType.Cyclops;

                SubControl subControl = sub.GetComponent<SubControl>();
                steeringWheelYaw = (float)subControl.ReflectionGet("steeringWheelYaw");
                steeringWheelPitch = (float)subControl.ReflectionGet("steeringWheelPitch");
                appliedThrottle = subControl.appliedThrottle && (bool)subControl.ReflectionGet("canAccel");
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
                                                                                        rightArmPosition,
                                                                                        health);
            return Optional.Of(model);
        }
    }
}
