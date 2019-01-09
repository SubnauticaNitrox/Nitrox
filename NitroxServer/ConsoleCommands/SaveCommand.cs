using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class SaveCommand : Command
    {
        public SaveCommand() : base("save")
        {
        }

        public override void RunCommand(string[] args, Player player)
        {
            Server.Instance.Save();
            player.SendPacket(new InGameMessageEvent("Saving World State"));
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
