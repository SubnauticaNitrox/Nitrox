using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerPasswordCommand : Command
    {
        private readonly SubnauticaServerConfig serverConfig;

        public ChangeServerPasswordCommand(SubnauticaServerConfig serverConfig) : base("changeserverpassword", Perms.ADMIN, "Changes server password. Clear it without argument")
        {
            AddParameter(new TypeString("password", false, "The new server password"));

            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            string password = args.Get(0) ?? string.Empty;

            using (serverConfig.Update(Path.Combine(Extensions.GetSavesFolderDir(), serverConfig.SaveName)))
            {
                serverConfig.ServerPassword = password;
            }

            Log.InfoSensitive("Server password changed to \"{password}\" by {playername}", password, args.SenderName);
            SendMessageToPlayer(args.Sender, "Server password has been updated");
        }
    }
}
