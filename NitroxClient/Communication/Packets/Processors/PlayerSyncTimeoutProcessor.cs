using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.InGame;
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
        // This will finish the loading screen
        Multiplayer.Main.InsideJoinQueue = false;
        Multiplayer.Main.InitialSyncCompleted = true;

        session.Disconnect();

        // TODO: make this translatable
        string message = "Initial sync timed out";

        Modal.Get<KickedModal>().Show(message);
    }
}
