using System.IO;
using Nitrox.Model.Packets;
using Nitrox.Model.Serialization;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;
internal class SetKeepInventoryCommand : Command
{
    private readonly PlayerManager playerManager;
    private readonly SubnauticaServerConfig serverConfig;
    private readonly Server server;

    public SetKeepInventoryCommand(PlayerManager playerManager, SubnauticaServerConfig serverConfig, Server server) : base("keepinventory", Nitrox.Model.DataStructures.GameLogic.Perms.ADMIN, "Sets \"keep inventory\" setting to on/off. If \"on\", players won't lose items when they die.")
    {
        AddParameter(new TypeBoolean("state", true, "The true/false state to set keep inventory on death to"));
        this.playerManager = playerManager;
        this.serverConfig = serverConfig;
        this.server = server;
    }

    protected override void Execute(CallArgs args)
    {
        bool newKeepInventoryState = args.Get<bool>(0);
        using (serverConfig.Update(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), server.Name)))
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
