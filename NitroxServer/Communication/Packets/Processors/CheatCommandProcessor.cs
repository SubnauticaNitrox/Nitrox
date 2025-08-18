using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

public class CheatCommandProcessor : AuthenticatedPacketProcessor<CheatCommand>
{
    public override void Process(CheatCommand packet, Player player)
    {
        if (player.Permissions < NitroxModel.DataStructures.GameLogic.Perms.MODERATOR)
        {
            Log.Warn($"{player.Name} used cheat command: '{packet.Command}' without sufficient permissions.");
            return;
        }

        Log.Info($"{player.Name} used cheat command : '{packet.Command}'");
    }
}
