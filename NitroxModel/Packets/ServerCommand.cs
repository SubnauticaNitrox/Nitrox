using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ServerCommand : Packet
    {
        public string Command { get; }

        public ServerCommand(string command)
        {
            Command = command;
        }

        public ServerCommand(string[] commandArgs)
        {
            Command = string.Join(" ", commandArgs);
        }

        public override string ToString()
        {
            return $"[ServerCommand - Command: {Command}]";
        }
    }
}
