using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours.Gui.Modals;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class ServerStoppedProcessor(IClient client) : IClientPacketProcessor<ServerStopped>
{
    private readonly IClient client = client;

    public Task Process(ClientProcessorContext context, ServerStopped packet)
    {
        // We can send the stop instruction right now instead of waiting for the timeout
        client.Stop();
        Modal.Get<ServerStoppedModal>()?.Show();
        return Task.CompletedTask;
    }
}
