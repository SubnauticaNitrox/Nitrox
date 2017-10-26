using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsChangeName : AuthenticatedPacket
    {
        public string Guid { get; }
        public string Name { get; }

        public CyclopsChangeName(string playerId, string guid, string name) : base(playerId)
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
