using Nitrox.Model.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.ChatUI;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class TextAutoCompleteProcessor : IClientPacketProcessor<TextAutoComplete>
{
    private readonly PlayerChatManager playerChatManager = PlayerChatManager.Instance;

    public Task Process(ClientProcessorContext context, TextAutoComplete packet)
    {
        if (string.IsNullOrWhiteSpace(packet.Text))
        {
            return Task.CompletedTask;
        }
        switch (packet.Context)
        {
            case TextAutoComplete.AutoCompleteContext.COMMAND_NAME:
                playerChatManager.SetChatInput($"/{packet.Text} ");
                break;
        }
        return Task.CompletedTask;
    }
}
