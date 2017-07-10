using NitroxModel.DataStructures.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public static class GuidHelper
    {
        public static Optional<GameObject> GetObjectFrom(String guid)
        {
            GameObject gameObject = UniqueIdentifierHelper.GetByName(guid);

            return Optional<GameObject>.OfNullable(gameObject);
        }

        public static String GetGuid(GameObject gameObject)
        {
            return GetUniqueIdentifier(gameObject).Id;
        }

        public static void SetNewGuid(GameObject gameObject, String guid)
        {
            GetUniqueIdentifier(gameObject).Id = guid;
        }

        private static UniqueIdentifier GetUniqueIdentifier(GameObject gameObject)
        {
            UniqueIdentifier uniqueIdentifier = gameObject.GetComponent<UniqueIdentifier>();

            if (uniqueIdentifier == null)
            {
                uniqueIdentifier = gameObject.AddComponent<UniqueIdentifier>();
            }

            return uniqueIdentifier;
        }
    }
}
