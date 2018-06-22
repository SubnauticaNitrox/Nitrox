using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class MovementSyncer : MonoBehaviour
    {
        public Vector3 NextPosition { get; private set; }
        public Quaternion NextRotation { get; private set; }

        public void SetNewLocation(Vector3 newPos, Quaternion newRot)
        {
            NextPosition = newPos;
            NextRotation = newRot;
        }

        public void Update()
        {
            transform.position = Vector3.Lerp(transform.position, NextPosition, PlayerMovement.BROADCAST_INTERVAL);
            transform.rotation = Quaternion.Lerp(transform.rotation, NextRotation, PlayerMovement.BROADCAST_INTERVAL);
        }
    }
}
