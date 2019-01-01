using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ServerCommand : Packet
    {
        public string[] CmdArgs;

        public ServerCommand(string[] args)
        {
            CmdArgs = args;
        }
    }
}
