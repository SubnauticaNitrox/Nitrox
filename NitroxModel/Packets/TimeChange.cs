using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class TimeChange : Packet
    {
        public double CurrentTime { get; }
        public bool InitialSync { get; }

        public TimeChange(double currentTime, bool initialSync)
        {
            CurrentTime = currentTime;
            InitialSync = initialSync;
        }
    }
}
