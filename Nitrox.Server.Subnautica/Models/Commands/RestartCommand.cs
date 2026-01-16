using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Helper;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class RestartCommand : Command
{
    public override IEnumerable<string> Aliases { get; } = ["reboot"];

    public RestartCommand() : base("restart", Perms.ADMIN, "Saves and restarts the server")
    {
    }

    protected override void Execute(CallArgs args)
    {
        Server server = Server.Instance;
        if (!server.IsRunning)
        {
            SendMessage(args.Sender, "Server is not running");
            return;
        }

        SendMessageToAllPlayers("Server is restarting...");
        Log.Info("Server restart requested via command");

        server.RequestRestart();
    }
}
