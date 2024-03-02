using NitroxClient.GameLogic;
using NitroxClient.Unity.Smoothing;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class RemotelyControlled : MonoBehaviour
{
    private readonly SmoothVector smoothPosition = new SmoothVector();
    private readonly SmoothRotation smoothRotation = new SmoothRotation();

    private SwimBehaviour swimBehaviour;
    private WalkBehaviour walkBehaviour;
    private Rigidbody rigidbody;

    public void Awake()
    {
        swimBehaviour = gameObject.GetComponent<SwimBehaviour>();
        walkBehaviour = gameObject.GetComponent<WalkBehaviour>();
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        if (swimBehaviour || walkBehaviour || !rigidbody)
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
        TeleportIfTooFar(position, rotation);

        if (swimBehaviour)
        {
            swimBehaviour.SwimTo(position, 3f);
        }

        Transform selfTransform = transform;

        // Entities can lose their swimBehavior (such as if they get killed).  Keep these up-to-date incase that happens.
        smoothPosition.Current = selfTransform.position;
        smoothRotation.Current = selfTransform.rotation;
        smoothPosition.Target = position;
        smoothRotation.Target = rotation;
    }

    public void UpdateKnownSplineUser(Vector3 currentPosition, Quaternion currentRotation, Vector3 destination, Vector3 desinationDirection, float velocity)
    {
        TeleportIfTooFar(currentPosition, currentRotation);

        if (swimBehaviour)
        {
            swimBehaviour.SwimToInternal(destination, desinationDirection, velocity, false);
        }

        if (walkBehaviour)
        {
            walkBehaviour.GoToInternal(destination, desinationDirection, velocity);
        }
    }

    private void TeleportIfTooFar(Vector3 position, Quaternion rotation)
    {
        Transform selfTransform = transform;

        if ((selfTransform.position - position).sqrMagnitude > 25) // Optimized 5m distance test
        {
            selfTransform.position = position;
            selfTransform.rotation = rotation;
        }
    }
}
