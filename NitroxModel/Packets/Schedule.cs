using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class Schedule : Packet
    {
        [Index(0)]
        public virtual float TimeExecute { get; protected set; }
        [Index(1)]
        public virtual string Key { get; protected set; }
        [Index(2)]
        public virtual string Type { get; protected set; }

        public Schedule() { }
        
        public Schedule(float timeExecute, string key, string type)
        {
            TimeExecute = timeExecute;
            Key = key;
            Type = type;
        }
    }
}
