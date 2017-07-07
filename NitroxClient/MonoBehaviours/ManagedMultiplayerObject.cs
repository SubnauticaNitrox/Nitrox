using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class ManagedMultiplayerObject : MonoBehaviour
    {
        private static Dictionary<String, GameObject> managedObjects = new Dictionary<String, GameObject>();

        public String GUID { get; protected set; }

        public void Awake()
        {
            String guid = Guid.NewGuid().ToString();
            ChangeGuid(guid);
        }

        public void ChangeGuid(string guid)
        {
            if(GUID != null)
            {
                managedObjects.Remove(GUID);
            }

            GUID = guid;
            managedObjects[GUID] = this.gameObject;
        }
        
    }
}
