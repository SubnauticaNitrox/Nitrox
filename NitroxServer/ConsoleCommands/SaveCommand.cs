using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands
{
    internal class SaveCommand : Command
    {
        public SaveCommand() : base("save", Perms.ADMIN, "", "Saves the map")
        {
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            Server.Instance.Save();
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
