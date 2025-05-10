using System;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record ServerCommand : Packet
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
