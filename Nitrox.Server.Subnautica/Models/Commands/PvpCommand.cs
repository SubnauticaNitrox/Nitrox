using System.IO;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Serialization;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands;

public class PvpCommand : Command
{
    private readonly SubnauticaServerConfig serverConfig;
    private readonly Server server;

    public PvpCommand(SubnauticaServerConfig serverConfig, Server server) : base("pvp", Perms.ADMIN, "Enables/Disables PvP")
    {
        AddParameter(new TypeString("state", true, "on/off"));

        this.serverConfig = serverConfig;
        this.server = server;
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


        using (serverConfig.Update(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), server.Name)))
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
