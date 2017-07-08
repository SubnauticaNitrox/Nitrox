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

        public String GUID { get; protected set; } = Guid.NewGuid().ToString();

        public void Awake()
        {
            managedObjects[GUID] = this.gameObject;
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
