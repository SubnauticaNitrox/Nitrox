using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.GameLogic.ManagedObjects
{
    public class MultiplayerObjectManager
    {
        private Dictionary<String, GameObject> objectsByGuid = new Dictionary<String, GameObject>();

        public Optional<GameObject> GetManagedObject(String guid)
        {
            if (objectsByGuid.ContainsKey(guid))
            {
                return Optional<GameObject>.Of(objectsByGuid[guid]);
            }
            
            return Optional<GameObject>.Empty();
        }

        public void SetupManagedObject(String guid, GameObject gameObject)
        {
            ManagedMultiplayerObject managedObject = gameObject.GetComponent<ManagedMultiplayerObject>();

            if(managedObject == null)
            {
                managedObject = gameObject.AddComponent<ManagedMultiplayerObject>();
                managedObject.ChangeGuid(guid);
            }

            objectsByGuid.Add(guid, gameObject);
        }
    }
}
