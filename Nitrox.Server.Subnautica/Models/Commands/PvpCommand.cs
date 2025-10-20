using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class PvpCommand : Command
{
    private readonly IOptions<SubnauticaServerOptions> options;

    public PvpCommand(IOptions<SubnauticaServerOptions> options) : base("pvp", Perms.ADMIN, "Enables/Disables PvP")
    {
        AddParameter(new TypeString("state", true, "on/off"));

        this.options = options;
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

        if (options.Value.PvpEnabled == pvpEnabled)
        {
            SendMessage(args.Sender, $"PvP is already {state}");
            return;
        }
        options.Value.PvpEnabled = pvpEnabled;
        SendMessageToAllPlayers($"PvP is now {state}");
    }
}
