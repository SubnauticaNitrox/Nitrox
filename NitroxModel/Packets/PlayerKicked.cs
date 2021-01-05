using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerKicked : Packet
    {
        public string Reason { get; }

        public PlayerKicked(string reason)
        {
            Reason = reason;
        }

        public override string ToString()
        {
            return $"[PlayerKicked - Reason : {Reason}]";
        }
    }
}
