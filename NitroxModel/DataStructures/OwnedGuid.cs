using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class OwnedGuid
    {
        public bool IsEntity { get; }
        public string Guid { get; }
        public string PlayerId { get; }

        public OwnedGuid(string guid, string playerId, bool isEntity)
        {
            Guid = guid;
            PlayerId = playerId;
            IsEntity = isEntity;
        }

        public override string ToString()
        {
            return "[OwnedGuid Guid: " + Guid + " PlayerId: " + PlayerId + " IsEntity: " + IsEntity + "]";
        }
    }
}
