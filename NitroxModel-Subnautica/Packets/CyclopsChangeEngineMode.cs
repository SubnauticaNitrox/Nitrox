using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsChangeEngineMode : Packet
    {
        public string Guid { get; }
        public CyclopsMotorMode.CyclopsMotorModes Mode { get; }

        public CyclopsChangeEngineMode(string guid, CyclopsMotorMode.CyclopsMotorModes mode)
        {
            Guid = guid;
            Mode = mode;
        }

        public override string ToString()
        {
            return "[CyclopsChangeEngineMode Guid: " + Guid + " Mode: " + Mode + "]";
        }
    }
}
