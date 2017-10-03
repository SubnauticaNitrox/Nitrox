using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class OpenableStateChanged : AuthenticatedPacket
    {
        public String Guid { get; }
        public bool IsOpen { get; }
        public float Duration { get; }

        public OpenableStateChanged(String playerId, String guid, bool isOpen, float duration) : base(playerId)
        {
            this.Guid = guid;
            this.IsOpen = isOpen;
            this.Duration = duration;
        }

        public override string ToString()
        {
            return "[OpenableStateChanged - PlayerId: " + PlayerId + " Guid: " + Guid + " IsOpen: " + IsOpen + " Duration: " + Duration + "]";
        }
    }
}
