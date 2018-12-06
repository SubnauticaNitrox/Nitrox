using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsActivateShield : Packet
    {
        public string Guid { get; }

        public CyclopsActivateShield(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsActivateShield Guid: " + Guid + "]";
        }
    }
}
