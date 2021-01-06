using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class AutoSaveCommand : Command
    {
        public AutoSaveCommand() : base("autosave", Perms.ADMIN, "Toggles the map autosave")
        {
            AddParameter(new TypeBoolean("on/off", true));
        }

        protected override void Execute(CallArgs args)
        {
            bool toggle = args.Get<bool>(0);
            ServerConfig serverConfig = NitroxConfig.Deserialize<ServerConfig>();
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
            NitroxConfig.Serialize(serverConfig);
        }
    }
}
