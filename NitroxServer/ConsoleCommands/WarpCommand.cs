using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class WarpCommand : Command
    {
        public WarpCommand() : base("warpto", Perms.ADMIN, "Teleports you on a player")
        {
            AddParameter(new TypePlayer("name", true));
        }

        protected override void Execute(CallArgs args)
        {
            Validate.IsTrue(args.Sender.HasValue, "This command can't be used by CONSOLE");

            Player otherPlayer = args.Get<Player>(0);
            args.Sender.Value.Teleport(otherPlayer.Position);

            SendMessage(args.Sender, $"Teleported to {otherPlayer.Name}");
        }
    }
}
