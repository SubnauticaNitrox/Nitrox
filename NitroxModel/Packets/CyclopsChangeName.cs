using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsChangeName : Packet
    {
        public string Guid { get; }
        public string Name { get; }

        public CyclopsChangeName(string guid, string name)
        {
            Guid = guid;
            Name = name;
        }

        public override string ToString()
        {
            return "[CyclopsChangeName Guid: " + Guid + " Name: " + Name + "]";
        }
    }
}
