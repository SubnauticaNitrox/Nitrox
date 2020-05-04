using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class GameModeChangedProcessor : ClientPacketProcessor<GameModeChanged>
    {
        public override void Process(GameModeChanged packet)
        {
            GameModeUtils.SetGameMode((GameModeOption)(int) packet.GameMode, GameModeOption.None);
        }
    }
}
