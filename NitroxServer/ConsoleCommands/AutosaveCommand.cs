using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxModel.Server;

namespace NitroxServer.ConsoleCommands
{
    internal class AutoSaveCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public AutoSaveCommand(ServerConfig serverConfig) : base("autosave", Perms.ADMIN, "Toggles the map autosave")
        {
            this.serverConfig = serverConfig;
            AddParameter(new TypeBoolean("on/off", true));
        }

        protected override void Execute(Optional<Player> sender)
        {
            bool toggle = ReadArgAt<bool>(0);

            if (toggle)
            {
                serverConfig.DisableAutoSave = false;
                Server.Instance.EnablePeriodicSaving();
                SendMessage(sender, "Enabled periodical saving");
            }
            else
            {
                serverConfig.DisableAutoSave = true;
                Server.Instance.DisablePeriodicSaving();
                SendMessage(sender, "Disabled periodical saving");
            }

        }
    }
}
