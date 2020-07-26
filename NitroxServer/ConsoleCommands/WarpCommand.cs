using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class WarpCommand : Command
    {
        public WarpCommand() : base("warp", Perms.ADMIN, "Allows to teleport players")
        {
            AddParameter(new TypePlayer("name", true));
            AddParameter(new TypePlayer("name", true));
        }

        protected override void Execute(CallArgs args)
        {
            Player player = args.Get<Player>(0);
            Player destination = args.Get<Player>(1);

            player.Teleport(destination.Position);
            SendMessage(player, $"Teleported to {destination.Name}");
        }
    }
}
