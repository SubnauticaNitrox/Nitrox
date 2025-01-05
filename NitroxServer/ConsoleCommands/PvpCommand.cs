using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands;

public class PvpCommand : Command
{
    private readonly ServerConfig serverConfig;

    public PvpCommand(ServerConfig serverConfig) : base("pvp", Perms.ADMIN, "Enables/Disables PvP")
    {
        AddParameter(new TypeString("state", true, "on/off"));

        this.serverConfig = serverConfig;
    }

    protected override void Execute(CallArgs args)
    {
        string state = args.Get<string>(0).ToLower();

        bool pvpEnabled = false;
        switch (state)
        {
            case "on":
                pvpEnabled = true;
                break;
            case "off":
                break;
            default:
                SendMessage(args.Sender, "Parameter must be \"on\" or \"off\"");
                return;
        }


        using (serverConfig.Update(Path.Combine(WorldManager.SavesFolderDir, serverConfig.SaveName)))
        {
            if (serverConfig.PvPEnabled == pvpEnabled)
            {
                SendMessage(args.Sender, $"PvP is already {state}");
                return;
            }

            serverConfig.PvPEnabled = pvpEnabled;

            SendMessageToAllPlayers($"PvP is now {state}");
        }
    }
}
