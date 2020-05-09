using System;
using NitroxModel.Server;

namespace NitroxModel.Packets
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
