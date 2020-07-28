using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class BroadcastCommand : Command
    {
        public BroadcastCommand() : base("broadcast", Perms.ADMIN, "Broadcasts a message on the server", true)
        {
            AddAlias("say");
            AddParameter(new TypeString("message", true));
        }

        protected override void Execute(CallArgs args)
        {
            SendMessageToAllPlayers(args.GetTillEnd());
        }
    }
}
