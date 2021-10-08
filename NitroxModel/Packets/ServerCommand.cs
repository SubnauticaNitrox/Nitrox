using System;

namespace NitroxModel.Packets
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
