using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ToggleLights : AuthenticatedPacket
    {
        public String Guid { get; }
        public bool IsOn { get; }

        public ToggleLights(String playerId, String guid, bool isOn) : base(playerId)
        {
            Guid = guid;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[ToggleLightsPacket PlayerId: " + PlayerId + " Guid: " + Guid + " IsOn: " + IsOn + "]";
        }
    }
}
