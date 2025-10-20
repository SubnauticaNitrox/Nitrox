﻿using System.IO;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Serialization;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class AutoSaveCommand : Command
    {
        private readonly Server server;
        private readonly SubnauticaServerConfig serverConfig;

        public AutoSaveCommand(Server server, SubnauticaServerConfig serverConfig) : base("autosave", Perms.ADMIN, "Toggles the map autosave")
        {
            AddParameter(new TypeBoolean("on/off", true, "Whether autosave should be on or off"));

            this.server = server;
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            bool toggle = args.Get<bool>(0);

            using (serverConfig.Update(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), server.Name)))
            {
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
}
