using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Cyclops;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class PlayerMovementBroadcaster : MonoBehaviour
{
    /// <remarks>
    /// In unity position units. Refer to <see cref="ShouldBroadcastMovement"/> for use infos.
    /// </remarks>
    private const float MINIMAL_MOVEMENT_THRESHOLD = 0.05f;
    /// <remarks>
    /// In degrees (°). Refer to <see cref="ShouldBroadcastMovement"/> for use infos.
    /// </remarks>
    private const float MINIMAL_ROTATION_THRESHOLD = 0.05f;
    /// <remarks>
    /// In units/s. Refer to <see cref="ShouldBroadcastMovement"/> for use infos.
    /// </remarks>
    private const float MINIMAL_VELOCITY_THRESHOLD = 0.1f;
    /// <remarks>
    /// In seconds. Refer to <see cref="ShouldBroadcastMovement"/> for use infos.
    /// </remarks>
    private const float MAX_TIME_WITHOUT_BROADCAST = 5f;
    /// <inheritdoc cref="MAX_TIME_WITHOUT_BROADCAST"/>
    private const float SAFETY_BROADCAST_WINDOW = 0.2f;

    private LocalPlayer localPlayer;

    private float latestBroadcastTime;
    private Vector3 latestPositionSent;
    private Quaternion latestBodyRotationSent;
    private bool hasSentStoppedPacket;

    public void Awake()
    {
        localPlayer = this.Resolve<LocalPlayer>();
    }

    public void Update()
    {
        // TODO: Replace this temporary fix. Mostly prevents server console being spammed with warnings when a client is in the queue.
        // There should be a way to block all packets from being sent when in the join queue or during initial sync.
        if (!Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }

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

        if (!ShouldBroadcastMovement(currentPosition, bodyRotation, playerVelocity))
        {
            return;
        }
        latestPositionSent = currentPosition;
        latestBodyRotationSent = bodyRotation;

        localPlayer.BroadcastLocation(currentPosition, playerVelocity, bodyRotation, aimingRotation);
    }

    /// <summary>
    /// Rate limiter which prevents non-moving players from spamming movement packets following some rules:
    /// - packets are never sent more often than <see cref="MovementBroadcaster.BROADCAST_PERIOD"/> allows
    /// - position changes less than <see cref="MINIMAL_MOVEMENT_THRESHOLD"/> and rotation changes less than <see cref="MINIMAL_ROTATION_THRESHOLD"/> are ignored
    /// - once the player comes to a full stop (velocity below <see cref="MINIMAL_VELOCITY_THRESHOLD"/>), exactly one final packet is sent so remote
    /// machines settle on the stopped state, after which broadcasting stops completely until the player moves again
    /// - if the player has velocity but hasn't moved past the position/rotation threshold yet (e.g. oscillating in place), a periodic resync every
    /// <see cref="MAX_TIME_WITHOUT_BROADCAST"/> (with a <see cref="SAFETY_BROADCAST_WINDOW"/> grace period) corrects any drift accumulating remotely
    /// </summary>
    private bool ShouldBroadcastMovement(Vector3 currentPosition, Quaternion bodyRotation, Vector3 velocity)
    {
        float currentTime = (float)this.Resolve<TimeManager>().RealTimeElapsed;
        if (currentTime < latestBroadcastTime + MovementBroadcaster.BROADCAST_PERIOD)
        {
            return false;
        }

        bool hasMoved = Vector3.Distance(latestPositionSent, currentPosition) > MINIMAL_MOVEMENT_THRESHOLD ||
                        Quaternion.Angle(latestBodyRotationSent, bodyRotation) > MINIMAL_ROTATION_THRESHOLD;
        if (hasMoved)
        {
            latestBroadcastTime = currentTime;
            hasSentStoppedPacket = false;
            return true;
        }

        bool isStandingStill = velocity.sqrMagnitude < MINIMAL_VELOCITY_THRESHOLD * MINIMAL_VELOCITY_THRESHOLD;
        if (isStandingStill)
        {
            if (hasSentStoppedPacket)
            {
                return false;
            }
            latestBroadcastTime = currentTime;
            hasSentStoppedPacket = true;
            return true;
        }

        if (currentTime > latestBroadcastTime + MAX_TIME_WITHOUT_BROADCAST)
        {
            if (currentTime > latestBroadcastTime + MAX_TIME_WITHOUT_BROADCAST + SAFETY_BROADCAST_WINDOW)
            {
                // only reset the broadcast timer after the safety window has elapsed, mirroring WatchedEntry.ShouldBroadcastMovement
                latestBroadcastTime = currentTime;
            }
            return true;
        }

        return false;
    }

    private bool BroadcastPlayerInCyclopsMovement()
    {
        if (!Player.main.isPiloting && Player.main.TryGetComponent(out CyclopsMotor cyclopsMotor) && cyclopsMotor.Pawn != null)
        {
            Transform pawnTransform = cyclopsMotor.Pawn.Handle.transform;
            PlayerInCyclopsMovement packet = new(this.Resolve<LocalPlayer>().SessionId.Value, pawnTransform.localPosition.ToDto(), pawnTransform.localRotation.ToDto());
            this.Resolve<IPacketSender>().Send(packet);
            return true;
        }
        return false;
    }
}
