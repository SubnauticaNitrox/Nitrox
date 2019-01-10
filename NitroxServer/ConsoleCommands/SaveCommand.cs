using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class SaveCommand : Command
    {
        public SaveCommand() : base("save", Perms.Admin)
        {
        }

        public override void RunCommand(string[] args)
        {
            Server.Instance.Save();
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
