using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public class CyclopsChangeSonarMode : Packet
    {
        public NitroxId Id { get; }
        public bool IsOn { get; }
        
        public CyclopsChangeSonarMode(NitroxId id, bool isOn)
        {
            Id = id;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return $"[CyclopsActivateSonar - Id: {Id}, IsOn: {IsOn}]";
        }
    }
}
