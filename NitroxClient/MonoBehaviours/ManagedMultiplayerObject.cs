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
        public bool BroadcastLocation { get; set; } = false;

        private float time = 0.0f;
        private float interpolationPeriod = 0.25f;

        public void Awake()
        {
            managedObjects[GUID] = this.gameObject;
        }

        public void Update()
        {
            if (BroadcastLocation)
            {
                time += Time.deltaTime;

                // Only do on a specific cadence to avoid hammering server
                if (time >= interpolationPeriod)
                {
                    time = 0;

                    Vector3 currentPosition = this.gameObject.transform.position;
                    Quaternion rotation = this.gameObject.transform.rotation;

                    Multiplayer.PacketSender.UpdateItemPosition(GUID, currentPosition, rotation);
                }
            }
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
