using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
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
