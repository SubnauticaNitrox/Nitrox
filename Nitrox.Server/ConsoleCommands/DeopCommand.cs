using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.ConsoleCommands.Abstract;
using Nitrox.Server.ConsoleCommands.Abstract.Type;

namespace Nitrox.Server.ConsoleCommands
{
    internal class DeopCommand : Command
    {
        public DeopCommand() : base("deop", Perms.ADMIN, "Removes admin rights from user")
        {
            AddParameter(new TypePlayer("name", true));
        }

        protected override void Execute(CallArgs args)
        {
            Player targetPlayer = args.Get<Player>(0);
            string playerName = args.Get(0);

            targetPlayer.Permissions = Perms.PLAYER;

            SendMessage(args.Sender, $"Updated {playerName}\'s permissions to PLAYER");
        }
    }
}
