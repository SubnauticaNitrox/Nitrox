using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsChangeName : AuthenticatedPacket
    {
        public String Guid { get; }
        public String Name { get; }

        public CyclopsChangeName(String playerId, String guid, string name) : base(playerId)
        {
            Guid = guid;
            Name = name;
        }

        public override string ToString()
        {
            return "[CyclopsChangeName PlayerId: " + PlayerId + " Guid: " + Guid + " Name: " + Name + "]";
        }
    }
}
