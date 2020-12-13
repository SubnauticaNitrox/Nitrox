using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class GameModeChangedProcessor : ClientPacketProcessor<GameModeChanged>
    {
        public override void Process(GameModeChanged packet)
        {
            GameModeUtils.SetGameMode((GameModeOption)(int) packet.GameMode, GameModeOption.None);
        }
    }
}
