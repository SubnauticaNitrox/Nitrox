using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.PlayerLogic;

public class PlayerCinematics
{
    private readonly IPacketSender packetSender;
    private readonly LocalPlayer localPlayer;

    public PlayerCinematics(IPacketSender packetSender, LocalPlayer localPlayer)
    {
        this.packetSender = packetSender;
        this.localPlayer = localPlayer;
    }

    public void StartCinematicMode(ushort playerId, NitroxId controllerID, int controllerNameHash, string key)
    {
        packetSender.Send(new PlayerCinematicControllerCall(playerId, controllerID, controllerNameHash, key, true));
    }

    public void EndCinematicMode(ushort playerId, NitroxId controllerID, int controllerNameHash, string key)
    {
        packetSender.Send(new PlayerCinematicControllerCall(playerId, controllerID, controllerNameHash, key, false));
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
