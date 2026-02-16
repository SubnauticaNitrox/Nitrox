using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class GameModeChangedProcessor(LocalPlayer localPlayer, PlayerManager playerManager) : IClientPacketProcessor<GameModeChanged>
{
    private readonly LocalPlayer localPlayer = localPlayer;
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, GameModeChanged packet)
    {
        if (packet.AllPlayers || packet.SessionId == localPlayer.SessionId)
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
        else if (playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer))
        {
            remotePlayer.SetGameMode(packet.GameMode);
        }
        return Task.CompletedTask;
    }
}
