using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class TimeChange : Packet
    {
        public double CurrentTime { get; }

        public TimeChange(double currentTime)
        {
            CurrentTime = currentTime;
        }

        public override string ToString()
        {
            return $"[TimeChange - CurrentTime: {CurrentTime}]";
        }
    }
}
