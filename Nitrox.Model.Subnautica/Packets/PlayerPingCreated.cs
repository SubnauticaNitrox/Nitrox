using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class PlayerPingCreated(SessionId sessionId, string text, NitroxVector3 position, NitroxId pingId, byte voiceLineIndex)
    : Packet
{
    public const byte VOICE_LINE_COUNT = 2;
    public const float MAX_VOICE_DISTANCE = 50f;

    public SessionId SessionId { get; } = sessionId;
    public string Text { get; } = text;
    public NitroxVector3 Position { get; } = position;
    public NitroxId PingId { get; } = pingId;
    public byte VoiceLineIndex { get; } = voiceLineIndex;
}
