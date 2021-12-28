using NitroxModel.Server;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class GameModeChanged : Packet
    {
        [Index(0)]
        public virtual ServerGameMode GameMode { get; protected set; }

        public GameModeChanged() { }

        public GameModeChanged(ServerGameMode gameMode)
        {
            GameMode = gameMode;
        }
    }
}
