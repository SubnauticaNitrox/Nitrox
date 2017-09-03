using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsChangeName : AuthenticatedPacket
    {
        public String Guid { get; private set; }
        public String Name { get; private set; }

        public CyclopsChangeName(String playerId, String guid, string name) : base(playerId)
        {
            this.Guid = guid;
            this.Name = name;
        }

        public override string ToString()
        {
            return "[CyclopsActivateShield PlayerId: " + PlayerId + " Guid: " + Guid + " Name: " + Name + "]";
        }
    }
}
