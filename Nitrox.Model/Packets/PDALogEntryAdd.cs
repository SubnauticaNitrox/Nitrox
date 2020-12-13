using System;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class PDALogEntryAdd : Packet
    {
        public string Key;
        public float Timestamp;
        public PDALogEntryAdd(string key, float timestamp)
        {
            Key = key;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return "[PDALogEntryAdd - Time: " + Timestamp + " Key: " + Key + "]";
        }
    }
}
