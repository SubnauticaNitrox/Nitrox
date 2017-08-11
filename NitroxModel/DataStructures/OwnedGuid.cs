using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class OwnedGuid
    {
        public String Guid;
        public String PlayerId;

        public OwnedGuid(String guid, String playerId)
        {
            this.Guid = guid;
            this.PlayerId = playerId;
        }

        public override string ToString()
        {
            return "[OwnedGuid Guid: " + Guid + " PlayerId: " + PlayerId + "]";
        }
    }
}
