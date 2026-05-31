using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours.Gui.Modals;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class UserKickedProcessor(IMultiplayerSession session) : IClientPacketProcessor<PlayerKicked>
{
    private readonly IMultiplayerSession session = session;

    public Task Process(ClientProcessorContext context, PlayerKicked packet)
    {
        string message = Language.main.Get("Nitrox_PlayerKicked");

        if (!string.IsNullOrEmpty(packet.Reason))
        {
            message += $"\n {packet.Reason}";
        }

        session.Disconnect();
        Modal.Get<KickedModal>().Show(message);
        return Task.CompletedTask;
    }
}
