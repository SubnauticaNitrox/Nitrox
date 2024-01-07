using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.PlayerLogic;

public class PlayerCinematics
{
    private readonly IPacketSender packetSender;
    private readonly LocalPlayer localPlayer;

    /// <summary>
    /// Some cinematics should not be played. Example the intro as it's completely handled by a dedicated system.
    /// </summary>
    private readonly HashSet<string> blacklistedKeys = ["escapepod_intro"];

    public PlayerCinematics(IPacketSender packetSender, LocalPlayer localPlayer)
    {
        this.packetSender = packetSender;
        this.localPlayer = localPlayer;
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
        if (localPlayer.IntroCinematicMode != introCinematicMode)
        {
            localPlayer.IntroCinematicMode = introCinematicMode;
            packetSender.Send(new SetIntroCinematicMode(localPlayer.PlayerId!.Value, introCinematicMode));
        }
    }
}
