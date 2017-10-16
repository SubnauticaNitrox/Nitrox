using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsToggleEngineState : AuthenticatedPacket
    {
        public String Guid { get; }
        public bool IsOn { get; }
        public bool IsStarting { get; }

        public CyclopsToggleEngineState(String playerId, String guid, bool isOn, bool isStarting) : base(playerId)
        {
            this.Guid = guid;
            this.IsOn = isOn;
            this.IsStarting = isStarting;
        }

        public override string ToString()
        {
            return "[CyclopsToggleEngineState PlayerId: " + PlayerId + " Guid: " + Guid + " IsOn: " + IsOn + " IsStarting: " + IsStarting + "]";
        }
    }
}