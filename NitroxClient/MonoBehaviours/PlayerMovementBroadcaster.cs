using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class PlayerMovementBroadcaster : MonoBehaviour
{
    private LocalPlayer localPlayer;

    public void Awake()
    {
        localPlayer = this.Resolve<LocalPlayer>();
    }

    public void Update()
    {
        // Freecam does disable main camera control
        // But it's also disabled when driving the cyclops through a cyclops camera (content.activeSelf is only true when controlling through a cyclops camera)
        if (!MainCameraControl.main.isActiveAndEnabled &&
            !uGUI_CameraCyclops.main.content.activeSelf)
        {
            return;
        }

        if (BroadcastPlayerInCyclopsMovement())
        {
            return;
        }

        Vector3 currentPosition = Player.main.transform.position;
        Vector3 playerVelocity = Player.main.playerController.velocity;

        // IDEA: possibly only CameraRotation is of interest, because bodyrotation is extracted from that.
        Quaternion bodyRotation = MainCameraControl.main.viewModel.transform.rotation;
        Quaternion aimingRotation = Player.main.camRoot.GetAimingTransform().rotation;

        Optional<VehicleMovementData> vehicle = GetVehicleMovement();
        SubRoot subRoot = Player.main.GetCurrentSub();

        // If in a subroot the position will be relative to the subroot
        if (subRoot)
        {
            // Rotate relative player position relative to the subroot (else there are problems with respawning)
            Transform subRootTransform = subRoot.transform;
            Quaternion undoVehicleAngle = subRootTransform.rotation.GetInverse();
            currentPosition = currentPosition - subRootTransform.position;
            currentPosition = undoVehicleAngle * currentPosition;
            bodyRotation = undoVehicleAngle * bodyRotation;
            aimingRotation = undoVehicleAngle * aimingRotation;
            currentPosition = subRootTransform.TransformPoint(currentPosition);

            if (Player.main.isPiloting && subRoot.isCyclops)
            {
                // In case you're driving the cyclops, the currentPosition is the real position of the player, so we need to send it to the server
                vehicle.Value.DriverPosition = currentPosition.ToDto();
            }
        }

        localPlayer.BroadcastLocation(currentPosition, playerVelocity, bodyRotation, aimingRotation, vehicle);
    }

    private bool BroadcastPlayerInCyclopsMovement()
    {
        if (!Player.main.isPiloting && Player.main.TryGetComponent(out CyclopsMotor cyclopsMotor) && cyclopsMotor.Pawn != null)
        {
            Transform pawnTransform = cyclopsMotor.Pawn.Handle.transform;
            PlayerInCyclopsMovement packet = new(this.Resolve<LocalPlayer>().PlayerId.Value, pawnTransform.localPosition.ToDto(), pawnTransform.localRotation.ToDto());
            this.Resolve<IPacketSender>().Send(packet);
            return true;
        }
        return false;
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
        bool appliedThrottle = false;
        Vector3 leftArmPosition = new(0, 0, 0);
        Vector3 rightArmPosition = new(0, 0, 0);
        float steeringWheelYaw, steeringWheelPitch;

        if (vehicle)
        {
            //TODO: We should cache this (and other MBs) somehow because the method is called very frequently
            if (!vehicle.TryGetIdOrWarn(out id))
            {
                return Optional.Empty;
            }

            Transform vehicleTransform = vehicle.transform;
            position = vehicleTransform.position;
            rotation = vehicleTransform.rotation;
            techType = CraftData.GetTechType(vehicle.gameObject);

            Rigidbody rigidbody = vehicle.GetComponent<Rigidbody>();

            velocity = rigidbody.velocity;
            angularVelocity = rigidbody.angularVelocity;

            // Required because vehicle is either a SeaMoth or an Exosuit, both types which can't see the fields either.
            steeringWheelYaw = vehicle.steeringWheelYaw;
            steeringWheelPitch = vehicle.steeringWheelPitch;

            // Vehicles (or the SeaMoth at least) do not have special throttle animations. Instead, these animations are always playing because the player can't even see them (unlike the cyclops which has cameras).
            // So, we need to hack in and try to figure out when thrust needs to be applied.
            if (AvatarInputHandler.main.IsEnabled())
            {
                if (techType == TechType.Seamoth)
                {
                    bool flag = position.y < Ocean.GetOceanLevel() && position.y < vehicle.worldForces.waterDepth && !vehicle.precursorOutOfWater;
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

                        Vector3 mainCameraPosition = MainCamera.camera.transform.position;
                        leftArmPosition = leftAim.transform.position = mainCameraPosition + quaternion * Vector3.forward * 100f;
                        rightArmPosition = rightAim.transform.position = mainCameraPosition + quaternion * Vector3.forward * 100f;
                    }
                }
            }
        }
        else if (sub && Player.main.isPiloting)
        {
            if (!sub.TryGetIdOrWarn(out id))
            {
                return Optional.Empty;
            }

            Transform subTransform = sub.transform;
            position = subTransform.position;
            rotation = subTransform.rotation;
            Rigidbody rigidbody = sub.GetComponent<Rigidbody>();
            velocity = rigidbody.velocity;
            angularVelocity = rigidbody.angularVelocity;
            techType = TechType.Cyclops;

            SubControl subControl = sub.GetComponent<SubControl>();
            steeringWheelYaw = subControl.steeringWheelYaw;
            steeringWheelPitch = subControl.steeringWheelPitch;
            appliedThrottle = subControl.appliedThrottle && subControl.canAccel;
        }
        else
        {
            return Optional.Empty;
        }

        return Optional.Of(
            VehicleMovementFactory.GetVehicleMovementData(
                techType,
                id,
                position,
                rotation,
                velocity,
                angularVelocity,
                steeringWheelYaw,
                steeringWheelPitch,
                appliedThrottle,
                leftArmPosition,
                rightArmPosition
            )
        );
    }
}
