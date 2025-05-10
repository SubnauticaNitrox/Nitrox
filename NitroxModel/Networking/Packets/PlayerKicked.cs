using System;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record PlayerKicked : Packet
    {
        public string Reason { get; }

        public PlayerKicked(string reason)
        {
            Reason = reason;
        }
    }
}
