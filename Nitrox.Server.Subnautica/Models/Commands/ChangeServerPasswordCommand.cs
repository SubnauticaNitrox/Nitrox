﻿using System.IO;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Serialization;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class ChangeServerPasswordCommand : Command
    {
        private readonly Server server;
        private readonly SubnauticaServerConfig serverConfig;

        public ChangeServerPasswordCommand(Server server, SubnauticaServerConfig serverConfig) : base("changeserverpassword", Perms.ADMIN, "Changes server password. Clear it without argument")
        {
            AddParameter(new TypeString("password", false, "The new server password"));

            this.server = server;
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            string password = args.Get(0) ?? string.Empty;

            using (serverConfig.Update(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), server.Name)))
            {
                serverConfig.ServerPassword = password;
            }

            Log.InfoSensitive("Server password changed to \"{password}\" by {playername}", password, args.SenderName);
            SendMessageToPlayer(args.Sender, "Server password has been updated");
        }
    }
}
