using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.ConsoleCommands.Abstract;
using Nitrox.Server.ConsoleCommands.Abstract.Type;
using Nitrox.Server.Serialization;

namespace Nitrox.Server.ConsoleCommands
{
    internal class AutoSaveCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public AutoSaveCommand(ServerConfig serverConfig) : base("autosave", Perms.ADMIN, "Toggles the map autosave")
        {
            this.serverConfig = serverConfig;
            AddParameter(new TypeBoolean("on/off", true));
        }

        protected override void Execute(CallArgs args)
        {
            bool toggle = args.Get<bool>(0);

            if (toggle)
            {
                serverConfig.DisableAutoSave = false;
                Server.Instance.EnablePeriodicSaving();
                SendMessage(args.Sender, "Enabled periodical saving");
            }
            else
            {
                serverConfig.DisableAutoSave = true;
                Server.Instance.DisablePeriodicSaving();
                SendMessage(args.Sender, "Disabled periodical saving");
            }
        }
    }
}
