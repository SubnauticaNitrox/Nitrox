using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class GameModeChangedProcessor : ClientPacketProcessor<GameModeChanged>
{
    private readonly LocalPlayer localPlayer;
    private readonly PlayerManager playerManager;

    public GameModeChangedProcessor(LocalPlayer localPlayer, PlayerManager playerManager)
    {
        this.localPlayer = localPlayer;
        this.playerManager = playerManager;
    }

    public override void Process(GameModeChanged packet)
    {
        if (packet.AllPlayers || packet.PlayerId == localPlayer.PlayerId)
        {
#if SUBNAUTICA
            GameModeUtils.SetGameMode((GameModeOption)(int)packet.GameMode, GameModeOption.None);
#elif BELOWZERO
            GameModeManager.SetGameOptions((GameModePresetId)(int)packet.GameMode);
#endif
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
    }
}
