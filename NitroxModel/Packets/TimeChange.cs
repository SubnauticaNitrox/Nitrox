using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class TimeChange : Packet
    {
        [Index(0)]
        public virtual double CurrentTime { get; protected set; }
        [Index(1)]
        public virtual bool InitialSync { get; protected set; }

        public TimeChange() { }

        public TimeChange(double currentTime, bool initialSync)
        {
            CurrentTime = currentTime;
            InitialSync = initialSync;
        }
    }
}
