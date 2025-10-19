using System;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class PlayerKicked : Packet
    {
        public string Reason { get; }

        public PlayerKicked(string reason)
        {
            Reason = reason;
        }
    }
}
