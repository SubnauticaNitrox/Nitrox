using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
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
