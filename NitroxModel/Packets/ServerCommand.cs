using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ServerCommand : Packet
    {
        public enum Commands
        {
            SAVE
        }

        public Commands Command { get; }

        public ServerCommand(Commands Command)
        {
            this.Command = Command;
        }
    }
}
