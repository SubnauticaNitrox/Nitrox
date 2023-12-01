using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class AutoBackupCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public AutoBackupCommand(ServerConfig serverConfig) : base("autobackup", Perms.ADMIN, "Toggles the map autobackup feature")
        {
            AddParameter(new TypeBoolean("on/off", true, "Whether backups should be made whenever the server is saved"));

            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            bool toggle = args.Get<bool>(0);

            serverConfig.Update(c =>
            {
                if (toggle)
                {
                    c.DisableAutoBackup = false;
                    Server.Instance.EnablePeriodicBackup();
                    SendMessage(args.Sender, "Enabled periodical backups");
                }
                else
                {
                    c.DisableAutoBackup = true;
                    Server.Instance.DisablePeriodicBackup();
                    SendMessage(args.Sender, "Disabled periodical backups");
                }
            });
        }
    }
}
