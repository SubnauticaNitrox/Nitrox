using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class ServerStoppedProcessor : ClientPacketProcessor<ServerStopped>
{
    private readonly IClient client;

    public ServerStoppedProcessor(IClient client)
    {
        this.client = client;
    }

    public override void Process(ServerStopped packet)
    {
        // We can send the stop instruction right now instead of waiting for the timeout
        client.Stop();
        Modal.Get<ServerStoppedModal>()?.Show();
    }
}
