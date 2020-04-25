using NitroxClient.GameLogic;
using NitroxClient.Unity.Smoothing;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class RemotelyControlled : MonoBehaviour
    {
        private SmoothVector smoothPosition = new SmoothVector(); 
        private SmoothRotation smoothRotation = new SmoothRotation();

        private SwimBehaviour swimBehaviour;
        private Rigidbody rigidbody;

        public void Awake()
        {
            swimBehaviour = gameObject.GetComponent<SwimBehaviour>();
            rigidbody = gameObject.GetComponent<Rigidbody>();
        }

        public void FixedUpdate()
        {
            if (swimBehaviour)
            {
                return;
            }

            smoothPosition.FixedUpdate();
            smoothRotation.FixedUpdate();

            rigidbody.isKinematic = false;
            rigidbody.velocity = MovementHelper.GetCorrectedVelocity(smoothPosition.Current, Vector3.zero, gameObject, EntityPositionBroadcaster.BROADCAST_INTERVAL);
            rigidbody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(smoothRotation.Current, Vector3.zero, gameObject, EntityPositionBroadcaster.BROADCAST_INTERVAL);
        }

        public void UpdateOrientation(Vector3 position, Quaternion rotation)
        {
            float distance = Vector3.Distance(gameObject.transform.position, position);

            if (distance > 5)
            {
                gameObject.transform.position = position;
                gameObject.transform.rotation = rotation;
            }
            
            if (swimBehaviour)
            {
                swimBehaviour.SwimTo(position, 3f);
            }

            // Entities can lose their swimBehavior (such as if they get killed).  Keep these up-to-date incase that happens.
            smoothPosition.Current = gameObject.transform.position;
            smoothRotation.Current = gameObject.transform.rotation;
            smoothPosition.Target = position;
            smoothRotation.Target = rotation;
        }
    }
}
