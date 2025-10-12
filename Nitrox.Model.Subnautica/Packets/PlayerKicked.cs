using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
