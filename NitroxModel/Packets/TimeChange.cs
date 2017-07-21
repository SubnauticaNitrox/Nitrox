using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class TimeChange : Packet
    {
        public float CurrentTime { get; private set; }

        public TimeChange(float currentTime) : base()
        {
            this.CurrentTime = currentTime;
        }
    }
}
