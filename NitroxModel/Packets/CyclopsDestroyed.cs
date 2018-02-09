using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsDestroyed : Packet
    {
        public string Guid { get; }

        public CyclopsDestroyed(string guid)
        {
            Guid = guid;
        }
    }
}
