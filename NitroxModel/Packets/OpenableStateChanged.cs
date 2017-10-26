using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class OpenableStateChanged : AuthenticatedPacket
    {
        public string Guid { get; }
        public bool IsOpen { get; }
        public float Duration { get; }

        public OpenableStateChanged(string playerId, string guid, bool isOpen, float duration) : base(playerId)
        {
            Guid = guid;
            IsOpen = isOpen;
            Duration = duration;
        }

        public override string ToString()
        {
            return "[OpenableStateChanged - PlayerId: " + PlayerId + " Guid: " + Guid + " IsOpen: " + IsOpen + " Duration: " + Duration + "]";
        }
    }
}
