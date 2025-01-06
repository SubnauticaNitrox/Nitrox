using System.IO;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

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
        SendMessageToAllPlayers($"KeepInventory has been updated to {args.Get<bool>(0)}");
        if (args.Get<bool>(0) == serverConfig.KeepInventoryOnDeath)
        {
            return;
        }
        serverConfig.KeepInventoryOnDeath = args.Get<bool>(0);
        serverConfig.Serialize(Path.Combine(WorldManager.SavesFolderDir, serverConfig.SaveName)); // Saves the server config edit to disk
        KeepInventoryChanged packet = new KeepInventoryChanged(args.Get<bool>(0));
        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            player.SendPacket(packet);
        }
    }
}
