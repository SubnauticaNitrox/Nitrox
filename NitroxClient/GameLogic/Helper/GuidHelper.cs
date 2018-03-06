using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public static class GuidHelper
    {
        public static GameObject RequireObjectFrom(string guid)
        {
            Optional<GameObject> gameObject = GetObjectFrom(guid);
            Validate.IsPresent(gameObject, "Game object required from guid: " + guid);
            return gameObject.Get();
        }

        // Feature parity of UniqueIdentifierHelper.GetByName() except does not do the verbose logging
        public static Optional<GameObject> GetObjectFrom(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return Optional<GameObject>.Empty();
            }

            UniqueIdentifier uniqueIdentifier;

            if (!UniqueIdentifier.TryGetIdentifier(guid, out uniqueIdentifier))
            {
                return Optional<GameObject>.Empty();
            }

            if (uniqueIdentifier == null)
            {
                return Optional<GameObject>.Empty();
            }

            return Optional<GameObject>.Of(uniqueIdentifier.gameObject);
        }

        public static string GetGuid(this GameObject gameObject)
        {
            return GetUniqueIdentifier(gameObject).Id;
        }

        public static void SetNewGuid(this GameObject gameObject, string guid)
        {
            GetUniqueIdentifier(gameObject).Id = guid;
        }

        private static UniqueIdentifier GetUniqueIdentifier(GameObject gameObject)
        {
            UniqueIdentifier uniqueIdentifier = gameObject.GetComponent<UniqueIdentifier>();

            if (uniqueIdentifier == null)
            {
                uniqueIdentifier = gameObject.AddComponent<PrefabIdentifier>();
            }

            return uniqueIdentifier;
        }
    }
}
