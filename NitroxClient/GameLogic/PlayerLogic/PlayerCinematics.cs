using System.Collections.Generic;
using Nitrox.Model.Core;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.GameLogic.PlayerLogic;

public class PlayerCinematics
{
    private readonly IPacketSender packetSender;
    private readonly LocalPlayer localPlayer;

    private IntroCinematicMode lastModeToSend = IntroCinematicMode.NONE;

    public SessionId? IntroCinematicPartnerId = null;

    /// <summary>
    /// Some cinematics should not be played. Example the intro as it's completely handled by a dedicated system.
    /// Bench and chair cinematics are handled by the BenchChanged packet system instead.
    /// </summary>
    private readonly HashSet<string> blacklistedKeys = ["escapepod_intro", "reaper_attack", "bench_sit", "bench_stand_up", "chair_sit", "chair_stand_up"];

    public PlayerCinematics(IPacketSender packetSender, LocalPlayer localPlayer)
    {
        this.packetSender = packetSender;
        this.localPlayer = localPlayer;
    }

    public void StartCinematicMode(SessionId sessionId, NitroxId controllerID, int controllerNameHash, string key, Dictionary<string, bool> animationParameters = null)
    {
        if (!blacklistedKeys.Contains(key))
        {
            packetSender.Send(new PlayerCinematicControllerCall(sessionId, controllerID, controllerNameHash, key, true, animationParameters));
        }
        else
        {
            Log.Debug($"[PlayerCinematics] Skipping blacklisted cinematic: {key}");
        }
    }

    public void EndCinematicMode(SessionId sessionId, NitroxId controllerID, int controllerNameHash, string key)
    {
        if (!blacklistedKeys.Contains(key))
        {
            packetSender.Send(new PlayerCinematicControllerCall(sessionId, controllerID, controllerNameHash, key, false));
        }
        else
        {
            Log.Debug($"[PlayerCinematics] Skipping blacklisted cinematic end: {key}");
        }
    }

    public void SetLocalIntroCinematicMode(IntroCinematicMode introCinematicMode)
    {
        if (!localPlayer.SessionId.HasValue)
        {
            Log.Error($"{nameof(SessionId)} was null while setting IntroCinematicMode to {introCinematicMode}");
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
            packetSender.Send(new SetIntroCinematicMode(localPlayer.SessionId.Value, introCinematicMode));
            return;
        }

        if (lastModeToSend == IntroCinematicMode.NONE)
        {
            Multiplayer.OnLoadingComplete += () => SetLocalIntroCinematicMode(lastModeToSend);
        }

        lastModeToSend = introCinematicMode;
    }
}
