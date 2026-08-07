using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class PlayerYell : Packet
{
    public const byte SOUND_COUNT = 24;
    public const float MAX_AUDIBLE_DISTANCE = 50f;

    public SessionId SessionId { get; }
    public byte SoundIndex { get; }
    public bool IsInsideVehicle { get; }

    public PlayerYell(SessionId sessionId, byte soundIndex, bool isInsideVehicle = false)
    {
        SessionId = sessionId;
        SoundIndex = soundIndex;
        IsInsideVehicle = isInsideVehicle;
    }

    public override string ToString()
    {
        return $"[{nameof(PlayerYell)} - {nameof(SessionId)}: {SessionId}, {nameof(SoundIndex)}: {SoundIndex}, {nameof(IsInsideVehicle)}: {IsInsideVehicle}]";
    }
}
