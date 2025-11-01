using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public class ServerCommand : Packet
    {
        public string Cmd { get; }

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
