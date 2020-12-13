using System;
using Nitrox.Model.Server;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class GameModeChanged : Packet
    {
        public ServerGameMode GameMode { get; }

        public GameModeChanged(ServerGameMode gameMode)
        {
            GameMode = gameMode;
        }
    }
}
