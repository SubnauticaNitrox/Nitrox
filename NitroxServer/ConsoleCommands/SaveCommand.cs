using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxModel.Server;

namespace NitroxServer.ConsoleCommands
{
    internal class SaveCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public SaveCommand(ServerConfig serverConfig) : base("save", Perms.ADMIN, "Saves the map")
        {
            this.serverConfig = serverConfig;
            AddParameter(new TypeBoolean("on/off", false));
        }

        protected override void Execute(Optional<Player> sender)
        {
            if (IsValidArgAt(0))
            {
                bool? toggle = ReadArgAt<bool?>(0);

                if (toggle.HasValue)
                {
                    if (toggle.Value)
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
            else
            {
                Server.Instance.Save();
                SendMessageToPlayer(sender, "World saved");
            }
        }
    }
}
