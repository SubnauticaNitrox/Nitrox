using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
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

        if (Player.main.isPiloting)
        {
            return;
        }

        Vector3 currentPosition = Player.main.transform.position;
        Vector3 playerVelocity = Player.main.playerController.velocity;

        // IDEA: possibly only CameraRotation is of interest, because bodyrotation is extracted from that.
        Quaternion bodyRotation = MainCameraControl.main.viewModel.transform.rotation;
        Quaternion aimingRotation = Player.main.camRoot.GetAimingTransform().rotation;

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
        }

        localPlayer.BroadcastLocation(currentPosition, playerVelocity, bodyRotation, aimingRotation);
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
}
