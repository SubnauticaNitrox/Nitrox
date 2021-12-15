using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Schedule : Packet
    {
        public float TimeExecute { get; }
        public string Key { get; }
        public string Type { get; }
        
        public Schedule(float timeExecute, string key, string type)
        {
            TimeExecute = timeExecute;
            Key = key;
            Type = type;
        }
    }
}
