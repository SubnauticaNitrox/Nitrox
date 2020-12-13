using System;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class TimeChange : Packet
    {
        public float CurrentTime { get; }

        public TimeChange(float currentTime)
        {
            CurrentTime = currentTime;
        }
    }
}
