using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
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

        public override string ToString()
        {
            return "[OpenableStateChanged - Id: " + Id + " IsOpen: " + IsOpen + " Duration: " + Duration + "]";
        }
    }
}
