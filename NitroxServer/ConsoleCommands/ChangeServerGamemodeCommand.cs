using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerGamemodeCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeServerGamemodeCommand(ServerConfig serverConfig) : base("changeservergamemode", Perms.ADMIN, "Changes server gamemode")
        {
            this.serverConfig = serverConfig;
            AddParameter(new TypeEnum<ServerGameMode>("gamemode", true));
        }

        protected override void Execute(CallArgs args)
        {
            ServerGameMode sgm = args.Get<ServerGameMode>(0);
            bool currentGamemodeHardcore = serverConfig.IsGameModeHardcore();

            serverConfig.GameModeEnum = sgm;
            SendMessage(args.Sender, $"Server gamemode changed to \"{sgm}\" by {args.SenderName}");
            
            if (serverConfig.IsGameModeHardcore())
            {
                serverConfig.DisableAutoSave = true;
                Server.Instance.DisablePeriodicSaving();
                SendMessage(args.Sender, "Disabled periodical saving");
            }
            else if (serverConfig.DisableAutoSave && currentGamemodeHardcore)
            {
                serverConfig.DisableAutoSave = false;
                Server.Instance.EnablePeriodicSaving();
                SendMessage(args.Sender, "Enabled periodical saving");
            }
        }
    }
}
