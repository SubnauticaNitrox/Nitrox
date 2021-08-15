using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class OpenableStateChanged : Packet
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
