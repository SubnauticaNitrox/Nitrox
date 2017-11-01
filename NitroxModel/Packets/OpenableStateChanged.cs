using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class OpenableStateChanged : Packet
    {
        public string Guid { get; }
        public bool IsOpen { get; }
        public float Duration { get; }

        public OpenableStateChanged(string guid, bool isOpen, float duration)
        {
            Guid = guid;
            IsOpen = isOpen;
            Duration = duration;
        }

        public override string ToString()
        {
            return "[OpenableStateChanged - Guid: " + Guid + " IsOpen: " + IsOpen + " Duration: " + Duration + "]";
        }
    }
}
