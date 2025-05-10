using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class GameModeChangedProcessor : IClientPacketProcessor<GameModeChanged>
{
    private readonly LocalPlayer localPlayer;
    private readonly PlayerManager playerManager;

    public GameModeChangedProcessor(LocalPlayer localPlayer, PlayerManager playerManager)
    {
        this.localPlayer = localPlayer;
        this.playerManager = playerManager;
    }

    public Task Process(IPacketProcessContext context, GameModeChanged packet)
    {
        if (packet.AllPlayers || packet.PlayerId == localPlayer.PlayerId)
        {
            GameModeUtils.SetGameMode((GameModeOption)(int)packet.GameMode, GameModeOption.None);
        }
        if (packet.AllPlayers)
        {
            foreach (RemotePlayer remotePlayer in playerManager.GetAll())
            {
                remotePlayer.SetGameMode(packet.GameMode);
            }
        }
        else if (playerManager.TryFind(packet.PlayerId, out RemotePlayer remotePlayer))
        {
            remotePlayer.SetGameMode(packet.GameMode);
        }

        return Task.CompletedTask;
    }
}
