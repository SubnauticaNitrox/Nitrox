using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class SyncedMultiplayerObject : MonoBehaviour
    {
        private Item itemBroadcaster;
        private float time;
        private const float INTERPOLATION_PERIOD = .25f;

        public void Awake()
        {
            itemBroadcaster = NitroxServiceLocator.LocateService<Item>();
        }

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= INTERPOLATION_PERIOD)
            {
                time = 0;

                string guid = GuidHelper.GetGuid(gameObject);
                Vector3 currentPosition = gameObject.transform.position;
                Quaternion rotation = gameObject.transform.rotation;

                itemBroadcaster.UpdatePosition(guid, currentPosition, rotation);
            }
        }

        public static void ApplyTo(GameObject gameObject)
        {
            gameObject.EnsureComponent<SyncedMultiplayerObject>();
        }
    }
}
