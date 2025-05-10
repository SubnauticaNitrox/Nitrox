using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record OpenableStateChanged : Packet
    {
        public NitroxId Id { get; }
        public bool IsOpen { get; }
        public float Duration { get; }

        public OpenableStateChanged(NitroxId id, bool isOpen, float duration)
        {
            Id = id;
            IsOpen = isOpen;
            Duration = duration;
        }
    }
}
