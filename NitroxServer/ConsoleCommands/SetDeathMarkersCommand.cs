using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands;

internal class SetDeathMarkersCommand : Command
{
    private readonly PlayerManager playerManager;
    private readonly ServerConfig serverConfig;

    public SetDeathMarkersCommand(PlayerManager playerManager, ServerConfig serverConfig) : base("setdeathmarkers", Perms.ADMIN, "Sets \"Death Markers\" setting to on/off. If \"on\", a beacon will appear at the location where a player dies.")
    {
        this.playerManager = playerManager;
        this.serverConfig = serverConfig;
        AddParameter(new TypeBoolean("state", true, "the on/off state of if a death marker is spawned on death"));
    }

    protected override void Execute(CallArgs args)
    {
        bool newDeathMarkersState = args.Get<bool>(0);
        using (serverConfig.Update(Path.Combine(WorldManager.SavesFolderDir, serverConfig.SaveName)))
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
