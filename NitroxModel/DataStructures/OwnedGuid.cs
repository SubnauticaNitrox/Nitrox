using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class OwnedGuid
    {
        // FUTURE: Find a more sophisticated way than `IsEntity` to determine the
        // handler/listener used for an ownership update.
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
