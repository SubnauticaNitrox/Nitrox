using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class ChatMessage : Packet
{
    public SessionId SessionId { get; }
    public string Text { get; }

    public ChatMessage(SessionId sessionId, string text)
    {
        SessionId = sessionId;
        Text = text;
    }
}
