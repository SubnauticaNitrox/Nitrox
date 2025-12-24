using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class CheatCommandProcessor : AuthenticatedPacketProcessor<CheatCommand>
{
    public override void Process(CheatCommand packet, Player player)
    {
        if (player.Permissions < Perms.MODERATOR)
        {
            Log.Warn($"{player.Name} used cheat command: '{packet.Command}' without sufficient permissions.");
            return;
        }

        Log.Info($"{player.Name} used cheat command: '{packet.Command}'");
    }
}
