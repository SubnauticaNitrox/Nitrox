using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsChangeSonarMode : Packet
    {
        public string Guid { get; }
        public bool IsOn { get; }
        
        public CyclopsChangeSonarMode(string guid, bool isOn)
        {
            Guid = guid;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[CyclopsActivateSonar Guid: " + Guid + "," + "isOn: " + IsOn + "]";
        }
    }
}
