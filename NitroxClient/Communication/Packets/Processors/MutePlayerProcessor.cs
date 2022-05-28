using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class MutePlayerProcessor : ClientPacketProcessor<MutePlayer>
{
    public delegate void PlayerMuted(ushort playerId, bool muted);
    public PlayerMuted OnPlayerMuted;

    public override void Process(MutePlayer packet)
    {
        OnPlayerMuted(packet.PlayerId, packet.Muted);
    }
}
