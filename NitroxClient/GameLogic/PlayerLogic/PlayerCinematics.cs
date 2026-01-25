using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.GameLogic.PlayerLogic;

public class PlayerCinematics
{
    private readonly IPacketSender packetSender;
    private readonly LocalPlayer localPlayer;

    private IntroCinematicMode lastModeToSend = IntroCinematicMode.NONE;
    private bool cinematicsHandlerRegistered;

    public ushort? IntroCinematicPartnerId = null;

    /// <summary>
    /// Some cinematics should not be played. Example the intro as it's completely handled by a dedicated system.
    /// </summary>
    private readonly HashSet<string> blacklistedKeys = ["escapepod_intro", "reaper_attack"];

    public PlayerCinematics(IPacketSender packetSender, LocalPlayer localPlayer)
    {
        this.packetSender = packetSender;
        this.localPlayer = localPlayer;

        // Register for cleanup when session ends
        Multiplayer.OnAfterMultiplayerEnd += CleanupCinematicsHandler;
    }

    public void StartCinematicMode(ushort playerId, NitroxId controllerID, int controllerNameHash, string key)
    {
        if (!blacklistedKeys.Contains(key))
        {
            packetSender.Send(new PlayerCinematicControllerCall(playerId, controllerID, controllerNameHash, key, true));
        }
    }

    public void EndCinematicMode(ushort playerId, NitroxId controllerID, int controllerNameHash, string key)
    {
        if (!blacklistedKeys.Contains(key))
        {
            packetSender.Send(new PlayerCinematicControllerCall(playerId, controllerID, controllerNameHash, key, false));
        }
    }

    public void SetLocalIntroCinematicMode(IntroCinematicMode introCinematicMode)
    {
        if (!localPlayer.PlayerId.HasValue)
        {
            Log.Error($"PlayerId was null while setting IntroCinematicMode to {introCinematicMode}");
            return;
        }

        if (localPlayer.IntroCinematicMode == introCinematicMode)
        {
            return;
        }

        localPlayer.IntroCinematicMode = introCinematicMode;

        // This method can be called before client is joined. To prevent sending as an unauthenticated packet we delay it.
        if (Multiplayer.Joined)
        {
            packetSender.Send(new SetIntroCinematicMode(localPlayer.PlayerId.Value, introCinematicMode));
            return;
        }

        if (lastModeToSend == IntroCinematicMode.NONE && !cinematicsHandlerRegistered)
        {
            Multiplayer.OnLoadingComplete += OnLoadingCompleteSendCinematicMode;
            cinematicsHandlerRegistered = true;
        }

        lastModeToSend = introCinematicMode;
    }

    private void OnLoadingCompleteSendCinematicMode()
    {
        SetLocalIntroCinematicMode(lastModeToSend);
    }

    private void CleanupCinematicsHandler()
    {
        if (cinematicsHandlerRegistered)
        {
            Multiplayer.OnLoadingComplete -= OnLoadingCompleteSendCinematicMode;
            cinematicsHandlerRegistered = false;
        }
        Multiplayer.OnAfterMultiplayerEnd -= CleanupCinematicsHandler;
        lastModeToSend = IntroCinematicMode.NONE;
    }
}
