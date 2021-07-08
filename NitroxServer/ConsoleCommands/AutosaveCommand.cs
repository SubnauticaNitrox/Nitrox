﻿using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class AutoSaveCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public AutoSaveCommand(ServerConfig serverConfig) : base("autosave", Perms.ADMIN, "Toggles the map autosave")
        {
            AddParameter(new TypeBoolean("on/off", true));

            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            bool toggle = args.Get<bool>(0);

            serverConfig.Update(c =>
            {
                if (toggle)
                {
                    c.DisableAutoSave = false;
                    Server.Instance.EnablePeriodicSaving();
                    SendMessage(args.Sender, "Enabled periodical saving");
                }
                else
                {
                    c.DisableAutoSave = true;
                    Server.Instance.DisablePeriodicSaving();
                    SendMessage(args.Sender, "Disabled periodical saving");
                }
            });
        }
    }
}
