using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsChangeEngineMode : AuthenticatedPacket
    {
        public string Guid { get; }
        public CyclopsMotorMode.CyclopsMotorModes Mode { get; }

        public CyclopsChangeEngineMode(string playerId, string guid, CyclopsMotorMode.CyclopsMotorModes mode) : base(playerId)
        {
            Guid = guid;
            Mode = mode;
        }

        public override string ToString()
        {
            return "[CyclopsChangeEngineMode PlayerId: " + PlayerId + " Guid: " + Guid + " Mode: " + Mode + "]";
        }
    }
}
