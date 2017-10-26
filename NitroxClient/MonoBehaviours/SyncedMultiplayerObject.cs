using System;
using NitroxClient.GameLogic.Helper;
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

                String guid = GuidHelper.GetGuid(gameObject);
                Vector3 currentPosition = gameObject.transform.position;
                Quaternion rotation = gameObject.transform.rotation;

                Multiplayer.Logic.Item.UpdatePosition(guid, currentPosition, rotation);
            }
        }

        public static void ApplyTo(GameObject gameObject)
        {
            gameObject.EnsureComponent<SyncedMultiplayerObject>();
        }
    }
}
