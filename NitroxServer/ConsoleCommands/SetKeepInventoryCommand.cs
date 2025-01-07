using System.IO;
using NitroxModel.Packets;
using NitroxModel.Server;
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

    public SetKeepInventoryCommand(PlayerManager playerManager, ServerConfig serverConfig) : base("keepinventory", NitroxModel.DataStructures.GameLogic.Perms.ADMIN, "Sets \"keep inventory\" setting to on/off. If \"on\", players won't lose items when they die.")
    {
        AddParameter(new TypeBoolean("state", true, "The true/false state to set keep inventory on death to"));
        this.playerManager = playerManager;
        this.serverConfig = serverConfig;
    }

    protected override void Execute(CallArgs args)
    {
        bool newKeepInventoryState = args.Get<bool>(0);
        using (serverConfig.Update(Path.Combine(WorldManager.SavesFolderDir, serverConfig.SaveName)))
        {
            if (serverConfig.KeepInventoryOnDeath != newKeepInventoryState)
            {
                serverConfig.KeepInventoryOnDeath = newKeepInventoryState;
                playerManager.SendPacketToAllPlayers(new KeepInventoryChanged(newKeepInventoryState));
                SendMessageToAllPlayers($"KeepInventoryOnDeath changed to \"{newKeepInventoryState}\" by {args.SenderName}");
            }
            else
            {
                SendMessage(args.Sender, $"KeepInventoryOnDeath already set to {newKeepInventoryState}");
            }
        }
    }
}
