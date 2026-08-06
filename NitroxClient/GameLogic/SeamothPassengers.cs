using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic;

/// <summary>
/// Maintains the local player's non-driving Seamoth passenger state.
/// Passenger state is only entered after the server acknowledges a request.
/// </summary>
public sealed class SeamothPassengers(IPacketSender packetSender, IMultiplayerSession multiplayerSession)
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly IMultiplayerSession multiplayerSession = multiplayerSession;

    private SeaMoth currentSeamoth;
    private Transform currentAnchor;
    private NitroxId currentSeamothId;
    private NitroxId pendingSeamothId;
    private bool passengerStateActive;

    public SessionId? LocalSessionId => multiplayerSession.Reservation?.SessionId;
    public SeaMoth CurrentSeamoth => currentSeamoth;
    public bool IsPassenger => passengerStateActive;

    public bool IsPassengerOf(SeaMoth seamoth)
    {
        if (!passengerStateActive || !seamoth)
        {
            return false;
        }

        return currentSeamoth && currentSeamoth == seamoth ||
               currentSeamothId != null && seamoth.TryGetNitroxId(out NitroxId seamothId) && currentSeamothId == seamothId;
    }

    public bool RequestEnter(SeaMoth seamoth)
    {
        if (!seamoth || !seamoth.enabled || seamoth.docked || IsPassenger || !seamoth.TryGetIdOrWarn(out NitroxId seamothId))
        {
            return false;
        }

        if (pendingSeamothId != null && pendingSeamothId.Equals(seamothId))
        {
            return true;
        }

        pendingSeamothId = seamothId;
        packetSender.Send(new SeamothPassengerStateChangeRequest(Optional.Of(seamothId)));
        return true;
    }

    public void ApplyState(SeamothPassengerStateChanged packet)
    {
        if (!LocalSessionId.HasValue || packet.SessionId != LocalSessionId.Value)
        {
            return;
        }

        pendingSeamothId = null;
        if (!packet.Accepted)
        {
            Log.Warn("The server rejected the Seamoth passenger request.");
        }

        if (!packet.SeamothId.HasValue)
        {
            ExitLocal(false, true);
            return;
        }

        if (!NitroxEntity.TryGetComponentFrom(packet.SeamothId.Value, out SeaMoth seamoth))
        {
            Log.Warn($"Could not find Seamoth {packet.SeamothId.Value} for the acknowledged passenger state.");
            ExitLocal(false, true);
            RequestExit();
            return;
        }

        EnterLocal(seamoth, packet.SeatIndex);
    }

    /// <summary>
    /// Handles the passenger exit prompt and suppresses ordinary player movement while mounted.
    /// </summary>
    public bool UpdatePassenger()
    {
        if (!passengerStateActive)
        {
            return false;
        }

        Player player = Player.main;
        if (!player || !currentSeamoth || !currentAnchor || player.mode != Player.Mode.LockedPiloting || player.transform.parent != currentAnchor)
        {
            ExitLocal(true, true);
            return false;
        }

        HandReticle.main.SetText(HandReticle.TextType.Hand, "PressToExit", true, GameInput.Button.Exit);
        HandReticle.main.SetIcon(HandReticle.IconType.None, 1f);
        if (GameInput.GetButtonDown(GameInput.Button.Exit))
        {
            ExitLocal(true, true);
        }
        return true;
    }

    public void OnLockedModeExited(Player player)
    {
        if (player == Player.main && IsPassenger)
        {
            ClearLocalState(true);
        }
    }

    public void OnVehicleUnavailable(Vehicle vehicle, bool notifyServer)
    {
        if (vehicle is SeaMoth seamoth && IsPassengerOf(seamoth))
        {
            ExitLocal(notifyServer, true);
        }
    }

    public void ExitLocal(bool notifyServer, bool placeOutside)
    {
        bool hadPassengerState = passengerStateActive || pendingSeamothId != null;
        SeaMoth exitingSeamoth = currentSeamoth;
        Transform exitingAnchor = currentAnchor;
        bool wasMountedPassenger = passengerStateActive;

        passengerStateActive = false;
        currentSeamoth = null;
        currentAnchor = null;
        currentSeamothId = null;
        pendingSeamothId = null;

        Player player = Player.main;
        if (player && wasMountedPassenger)
        {
            player.inSeamoth = false;
            player.sitting = false;

            if (player.mode == Player.Mode.LockedPiloting)
            {
                if (placeOutside && exitingSeamoth)
                {
                    if (!player.SpawnNearby(3f, exitingSeamoth.gameObject))
                    {
                        player.transform.position = exitingSeamoth.transform.position + exitingSeamoth.transform.up * 1.5f + exitingSeamoth.transform.right * 1.5f;
                    }
                }
                player.ExitLockedMode(false, false);
            }
            else if (player.mode != Player.Mode.Normal)
            {
                player.ToNormalMode(false);
            }

            // Unity may destroy or reparent the anchor before its locked-mode state changes. Never leave a tracked
            // passenger parented after cleanup, even when ExitLockedMode was no longer applicable.
            if (player.transform.parent)
            {
                player.transform.SetParent(null, true);
            }

            player.playerController.SetEnabled(true);
            player.playerController.UpdateController();
        }

        SeamothPassengerAnchors.RemoveIfEmpty(exitingAnchor);
        if (notifyServer && hadPassengerState)
        {
            RequestExit();
        }
    }

    private void EnterLocal(SeaMoth seamoth, byte seatIndex)
    {
        if (!seamoth ||
            seamoth.docked ||
            seatIndex >= SeamothPassengerAnchors.MaxPassengers ||
            !Player.main ||
            !seamoth.TryGetNitroxId(out NitroxId seamothId))
        {
            RequestExit();
            return;
        }

        Transform anchor = SeamothPassengerAnchors.GetOrCreate(seamoth, seatIndex);
        if (IsPassengerOf(seamoth) && currentAnchor == anchor)
        {
            return;
        }

        if (Player.main.mode != Player.Mode.Normal)
        {
            RequestExit();
            return;
        }

        if (IsPassenger)
        {
            ExitLocal(false, true);
        }

        Player player = Player.main;
        passengerStateActive = true;
        currentSeamoth = seamoth;
        currentAnchor = anchor;
        currentSeamothId = seamothId;

        player.SetCurrentSub(null, false);
        player.playerController.UpdateController();
        player.inSeamoth = true;
        player.sitting = seamoth.playerSits;
        player.EnterLockedMode(anchor, true);
    }

    private void ClearLocalState(bool notifyServer)
    {
        bool hadPassengerState = passengerStateActive || pendingSeamothId != null;
        bool wasMountedPassenger = passengerStateActive;
        Transform exitingAnchor = currentAnchor;
        passengerStateActive = false;
        currentSeamoth = null;
        currentAnchor = null;
        currentSeamothId = null;
        pendingSeamothId = null;

        if (Player.main)
        {
            Player.main.inSeamoth = false;
            Player.main.sitting = false;

            if (wasMountedPassenger && Player.main.mode != Player.Mode.LockedPiloting)
            {
                if (Player.main.mode != Player.Mode.Normal)
                {
                    Player.main.ToNormalMode(false);
                }
                if (Player.main.transform.parent)
                {
                    Player.main.transform.SetParent(null, true);
                }
            }
        }

        SeamothPassengerAnchors.RemoveIfEmpty(exitingAnchor);

        if (notifyServer && hadPassengerState)
        {
            RequestExit();
        }
    }

    private void RequestExit() => packetSender.Send(new SeamothPassengerStateChangeRequest(Optional.Empty));
}
