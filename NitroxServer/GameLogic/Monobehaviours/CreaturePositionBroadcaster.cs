using UnityEngine;

namespace NitroxServer.GameLogic.Monobehaviours
{
    public class CreaturePositionBroadcaster : MonoBehaviour
    {
        public const float BROADCAST_INTERVAL = 0.05f;

        private float time = 0.0f;

        public void Update()
        {
            time += Time.deltaTime;

            if (time >= BROADCAST_INTERVAL)
            {
                time = 0;
                Server.Logic.AI.BroadcastMovedCreatures();
            }
        }
    }
}
