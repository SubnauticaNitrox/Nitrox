using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
{
    private readonly JoiningManager joiningManager;

    public PlayerJoiningMultiplayerSessionProcessor(JoiningManager joiningManager)
    {
        this.joiningManager = joiningManager;
    }

    public override void Process(PlayerJoiningMultiplayerSession packet, INitroxConnection connection)
    {
        joiningManager.AddToJoinQueue(connection, packet.ReservationKey);
    }
}
