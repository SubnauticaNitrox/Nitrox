using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class PlayerBannedProcessor : ClientPacketProcessor<PlayerBannedEvent>
    {
        public override void Process(PlayerBannedEvent playerBanned)
        {
            Log.InGame(playerBanned.PlayerName + " has been banned");
        }
    }
}
