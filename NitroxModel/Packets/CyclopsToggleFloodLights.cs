using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsToggleFloodLights : AuthenticatedPacket
    {
        public String Guid { get; private set; }
        public bool IsOn { get; set; }

        public CyclopsToggleFloodLights(String playerId, String guid, bool isOn) : base(playerId)
        {
            this.Guid = guid;
            this.IsOn = isOn;
        }

        public override string ToString()
        {
            return "[CyclopsToggleFloodLights PlayerId: " + PlayerId + " Guid: " + Guid + " IsOn: " + IsOn + "]";
        }
    }
}
