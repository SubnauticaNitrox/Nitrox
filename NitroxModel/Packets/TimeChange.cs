using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class TimeChange : Packet
    {
        public float CurrentGameTime { get; }
        public double CurrentServerUtcTimeMilliseconds { get; }
        public double ClientSentAtMilliseconds { get; }

        public TimeChange(float currentGameTime, double currentServerUtcTimeMilliseconds, double clientSentAtMilliseconds = 0)
        {
            CurrentGameTime = currentGameTime;
            CurrentServerUtcTimeMilliseconds = currentServerUtcTimeMilliseconds;
            ClientSentAtMilliseconds = clientSentAtMilliseconds;
        }
    }
}
