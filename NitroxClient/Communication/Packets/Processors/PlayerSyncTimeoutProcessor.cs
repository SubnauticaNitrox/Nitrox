using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerSyncTimeoutProcessor : ClientPacketProcessor<PlayerSyncTimeout>
{
    private readonly IMultiplayerSession session;

    public PlayerSyncTimeoutProcessor(IMultiplayerSession session)
    {
        this.session = session;
    }

    public override void Process(PlayerSyncTimeout packet)
    {
        // This will advance the coroutine in Multiplayer::LoadAsync() which quits to menu
        Multiplayer.Main.InitialSyncCompleted = true;
        Multiplayer.Main.TimedOut = true;

        session.Disconnect();
    }
}
