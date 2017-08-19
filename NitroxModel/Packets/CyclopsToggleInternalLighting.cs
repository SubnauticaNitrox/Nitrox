using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsToggleInternalLighting : AuthenticatedPacket
    {
        public String Guid { get; private set; }
        public bool IsOn { get; private set; }

        public CyclopsToggleInternalLighting(String playerId, String guid, bool isOn) : base(playerId)
        {
            this.Guid = guid;
            this.IsOn = isOn;
        }
        
        public override string ToString()
        {
            return "[CyclopsToggleInternalLighting PlayerId: " + PlayerId + " Guid: " + Guid + " IsOn: " + IsOn + "]";
        }
    }
}
