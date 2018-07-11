using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConsoleEntry : Packet
    {
        public string Command { get; }

        public ConsoleEntry(string Command)
        {
            this.Command = Command;
        }
    }
}
