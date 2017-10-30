using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsToggleEngineState : AuthenticatedPacket
    {
        public string Guid { get; }
        public bool IsOn { get; }
        public bool IsStarting { get; }

        public CyclopsToggleEngineState(string playerId, string guid, bool isOn, bool isStarting) : base(playerId)
        {
            Guid = guid;
            IsOn = isOn;
            IsStarting = isStarting;
        }

        public override string ToString()
        {
            return "[CyclopsToggleEngineState PlayerId: " + PlayerId + " Guid: " + Guid + " IsOn: " + IsOn + " IsStarting: " + IsStarting + "]";
        }
    }
}
