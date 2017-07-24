using NitroxClient.GameLogic.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class SyncedMultiplayerObject : MonoBehaviour
    {
        private float time = 0.0f;
        private float interpolationPeriod = 0.25f;
        
        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= interpolationPeriod)
            {
                time = 0;

                String guid = GuidHelper.GetGuid(this.gameObject);
                Vector3 currentPosition = this.gameObject.transform.position;
                Quaternion rotation = this.gameObject.transform.rotation;

                Multiplayer.Logic.Item.UpdatePosition(guid, currentPosition, rotation);
            }            
        }

        public static void ApplyTo(GameObject gameObject)
        {
            SyncedMultiplayerObject synced = gameObject.GetComponent<SyncedMultiplayerObject>();

            if (synced == null)
            {
               gameObject.AddComponent<SyncedMultiplayerObject>();
            }
        }
    }
}
