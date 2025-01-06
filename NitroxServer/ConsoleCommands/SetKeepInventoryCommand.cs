using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands;
internal class SetKeepInventoryCommand : Command
{
    private readonly PlayerManager playerManager;
    private readonly ServerConfig serverConfig;
    public SetKeepInventoryCommand(PlayerManager playerManager, ServerConfig serverConfig) : base("setkeepinventory", NitroxModel.DataStructures.GameLogic.Perms.ADMIN, "Sets the keep inventory setting to either true or false")
    {
        AddParameter(new TypeBoolean("state", true, "The true/false state to set keep inventory to"));
        this.playerManager = playerManager;
        this.serverConfig = serverConfig;
    }
    protected override void Execute(CallArgs args)
    {
        serverConfig.KeepInventory = args.Get<bool>(0);
        KeepInventoryChanged packet = new KeepInventoryChanged(args.Get<bool>(0));
        foreach(Player player in playerManager.GetConnectedPlayers())
        {
            player.SendPacket(packet);
        }
        SendMessageToAllPlayers($"KeepInventory has been updated to {args.Get<bool>(0)}");
    }
}
