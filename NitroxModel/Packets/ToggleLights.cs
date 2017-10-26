using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ToggleLights : AuthenticatedPacket
    {
        public String Guid { get; private set; }
        public bool IsOn { get; private set; }

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
