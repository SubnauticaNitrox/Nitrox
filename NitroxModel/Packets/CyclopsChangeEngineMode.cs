using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsChangeEngineMode : AuthenticatedPacket
    {
        public String Guid { get; }
        public CyclopsMotorMode.CyclopsMotorModes Mode { get; }

        public CyclopsChangeEngineMode(String playerId, String guid, CyclopsMotorMode.CyclopsMotorModes mode) : base(playerId)
        {
            this.Guid = guid;
            this.Mode = mode;
        }

        public override string ToString()
        {
            return "[CyclopsChangeEngineMode PlayerId: " + PlayerId + " Guid: " + Guid + " Mode: " + Mode + "]";
        }
    }
}
