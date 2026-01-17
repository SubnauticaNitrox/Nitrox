using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;
internal class SetKeepInventoryCommand : Command
{
    private readonly PlayerManager playerManager;
    private readonly IOptions<SubnauticaServerOptions> options;

    public SetKeepInventoryCommand(PlayerManager playerManager, IOptions<SubnauticaServerOptions> options) : base("keepinventory", Perms.ADMIN, "Sets \"keep inventory\" setting to on/off. If \"on\", players won't lose items when they die.")
    {
        AddParameter(new TypeBoolean("state", true, "The true/false state to set keep inventory on death to"));
        this.playerManager = playerManager;
        this.options = options;
    }

    protected override void Execute(CallArgs args)
    {
        bool newKeepInventoryState = args.Get<bool>(0);
        if (options.Value.KeepInventoryOnDeath != newKeepInventoryState)
        {
            options.Value.KeepInventoryOnDeath = newKeepInventoryState;
            playerManager.SendPacketToAllPlayers(new KeepInventoryChanged(newKeepInventoryState));
            SendMessageToAllPlayers($"KeepInventoryOnDeath changed to \"{newKeepInventoryState}\" by {args.SenderName}");
        }
        else
        {
            SendMessage(args.Sender, $"KeepInventoryOnDeath already set to {newKeepInventoryState}");
        }
    }
}
