using System;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class ServerCommand : Packet
    {
        public readonly string Cmd;

        public ServerCommand(string cmd)
        {
            Cmd = cmd;
        }

        public ServerCommand(string[] cmdArgs)
        {
            Cmd = string.Join(" ", cmdArgs);
        }
    }
}
