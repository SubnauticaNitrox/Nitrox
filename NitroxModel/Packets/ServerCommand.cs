using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ServerCommand : Packet
    {
        [Index(0)]
        public virtual string Cmd { get; protected set; }

        public ServerCommand() { }

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
