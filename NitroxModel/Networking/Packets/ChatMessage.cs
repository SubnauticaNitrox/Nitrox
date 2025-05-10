using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record ChatMessage : Packet
{
    public SessionId SenderId { get; }
    public string Text { get; }

    public ChatMessage(SessionId senderId, string text)
    {
        SenderId = senderId;
        Text = text;
    }
}
