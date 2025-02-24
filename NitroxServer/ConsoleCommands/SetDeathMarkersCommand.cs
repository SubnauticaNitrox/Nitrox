using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands;

internal class SetDeathMarkersCommand : Command
{
    private readonly PlayerManager playerManager;
    private readonly SubnauticaServerConfig serverConfig;
    private readonly Server server;

    public SetDeathMarkersCommand(PlayerManager playerManager, SubnauticaServerConfig serverConfig, Server server) : base("setdeathmarkers", Perms.ADMIN, "Sets \"Death Markers\" setting to on/off. If \"on\", a beacon will appear at the location where a player dies.")
    {
        this.playerManager = playerManager;
        this.serverConfig = serverConfig;
        this.server = server;
        AddParameter(new TypeBoolean("state", true, "on/off to enable/disable death markers"));
    }

    protected override void Execute(CallArgs args)
    {
        bool newDeathMarkersState = args.Get<bool>(0);
        using (serverConfig.Update(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), server.Name)))
        {
            if (serverConfig.MarkDeathPointsWithBeacon != newDeathMarkersState)
            {
                serverConfig.MarkDeathPointsWithBeacon = newDeathMarkersState;
                playerManager.SendPacketToAllPlayers(new DeathMarkersChanged(newDeathMarkersState));
                SendMessageToAllPlayers($"MarkDeathPointsWithBeacon changed to \"{newDeathMarkersState}\" by {args.SenderName}");
            }
            else
            {
                SendMessage(args.Sender, $"MarkDeathPointsWithBeacon already set to {newDeathMarkersState}");
            }
        }
    }
}
