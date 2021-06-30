using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerPasswordCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeServerPasswordCommand(ServerConfig serverConfig) : base("changeserverpassword", Perms.ADMIN, "Changes server password. Clear it without argument")
        {
            AddParameter(new TypeString("password", false));

            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            string password = args.Get(0) ?? string.Empty;

            serverConfig.ServerPassword = password;
            serverConfig.Serialize();

            Log.InfoSensitive("Server password changed to \"{password}\" by {playername}", password, args.SenderName);
            SendMessageToPlayer(args.Sender, "Server password has been updated");
        }
    }
}
