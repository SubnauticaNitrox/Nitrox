using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.Modals;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class ServerStoppedProcessor : IClientPacketProcessor<ServerStopped>
{
    private readonly IClient client;

    public ServerStoppedProcessor(IClient client)
    {
        this.client = client;
    }

    public Task Process(IPacketProcessContext context, ServerStopped packet)
    {
        // We can send the stop instruction right now instead of waiting for the timeout
        client.Stop();
        Modal.Get<ServerStoppedModal>()?.Show();

        return Task.CompletedTask;
    }
}
