using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
