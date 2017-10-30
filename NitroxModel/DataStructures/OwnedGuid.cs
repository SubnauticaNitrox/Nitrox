using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class OwnedGuid
    {
        public string Guid { get; }
        public string PlayerId { get; }

        public OwnedGuid(string guid, string playerId)
        {
            Guid = guid;
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return "[OwnedGuid Guid: " + Guid + " PlayerId: " + PlayerId + "]";
        }
    }
}
