using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class OwnedGuid
    {
        public String Guid { get; }
        public String PlayerId { get; }

        public OwnedGuid(String guid, String playerId)
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
