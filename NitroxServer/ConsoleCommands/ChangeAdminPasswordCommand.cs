using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeAdminPasswordCommand : Command
    {
        public ChangeAdminPasswordCommand() : base("changeadminpassword", Perms.ADMIN, "Changes admin password")
        {
            AddParameter(new TypeString("password", true));
        }

        protected override void Execute(CallArgs args)
        {
            string newPassword = args.Get(0);

            ServerConfig serverConfig = NitroxConfig.Deserialize<ServerConfig>();
            serverConfig.AdminPassword = newPassword;
            NitroxConfig.Serialize(serverConfig);

            Log.InfoSensitive("Admin password changed to {password} by {playername}", newPassword, args.SenderName);
            SendMessageToPlayer(args.Sender, "Admin password changed. In order to take effect pls restart the server.");
        }
    }
}
