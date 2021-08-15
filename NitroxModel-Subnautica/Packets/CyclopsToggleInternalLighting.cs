using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsToggleInternalLighting : Packet
    {
        public NitroxId Id { get; }
        public bool IsOn { get; }

        public CyclopsToggleInternalLighting(NitroxId id, bool isOn)
        {
            Id = id;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return $"[CyclopsToggleInternalLighting - Id: {Id}, IsOn: {IsOn}]";
        }
    }
}
