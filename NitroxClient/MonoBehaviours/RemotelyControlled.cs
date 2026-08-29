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
    private WorldForces worldForces;

    public void Awake()
    {
        swimBehaviour = gameObject.GetComponent<SwimBehaviour>();
        walkBehaviour = gameObject.GetComponent<WalkBehaviour>();
        rigidbody = gameObject.GetComponent<Rigidbody>();
        worldForces = gameObject.GetComponent<WorldForces>();

        if (rigidbody)
        {
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        if (worldForces)
        {
            worldForces.enabled = false;
        }
    }

    public void OnDestroy()
    {
        if (worldForces)
        {
            worldForces.enabled = true;
        }
    }

    public void FixedUpdate()
    {
        // (WalkBehaviour inherits from SwimBehaviour)
        if (swimBehaviour && swimBehaviour.enabled)
        {
            return;
        }

        smoothPosition.FixedUpdate();
        smoothRotation.FixedUpdate();

        if (rigidbody)
        {
            if (rigidbody.isKinematic)
            {
                rigidbody.isKinematic = false;
            }
            rigidbody.velocity = MovementHelper.GetCorrectedVelocity(smoothPosition.Current, Vector3.zero, gameObject, EntityPositionBroadcaster.BROADCAST_INTERVAL);
            rigidbody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(smoothRotation.Current, Vector3.zero, gameObject, EntityPositionBroadcaster.BROADCAST_INTERVAL);
        }
        else
        {
            transform.position = smoothPosition.Current;
            transform.rotation = smoothRotation.Current;
        }
    }

    public void UpdateOrientation(Vector3 position, Quaternion rotation)
    {
        bool teleported = TeleportIfTooFar(position, rotation);

        if (swimBehaviour && swimBehaviour.enabled)
        {
            swimBehaviour.SwimTo(position, 3f);

            smoothPosition.Current = transform.position;
            smoothRotation.Current = transform.rotation;
        }
        else if (teleported)
        {
            smoothPosition.Current = position;
            smoothRotation.Current = rotation;
        }

        // Entities can lose their swimBehavior (such as if they get killed).  Keep these up-to-date incase that happens.
        smoothPosition.Target = position;
        smoothRotation.Target = rotation;
    }

    public void UpdateKnownSplineUser(Vector3 currentPosition, Quaternion currentRotation, Vector3 destination, Vector3 destinationDirection, float velocity)
    {
        TeleportIfTooFar(currentPosition, currentRotation);

        if (swimBehaviour && swimBehaviour.enabled)
        {
            // First lines of SwimBehaviour.SwimToInternal
            swimBehaviour.originalTargetPosition = destination;
            swimBehaviour.originalTargetDirection = destinationDirection;
            swimBehaviour.originalVelocity = velocity;
            // Only the useful part of the methods called in SwimBehaviour.SwimToInternal
            swimBehaviour.splineFollowing.GoTo(destination, destinationDirection, velocity);
        }

        if (walkBehaviour && walkBehaviour.enabled)
        {
            walkBehaviour.GoToInternal(destination, destinationDirection, velocity);
        }
    }

    private bool TeleportIfTooFar(Vector3 position, Quaternion rotation)
    {
        if ((transform.position - position).sqrMagnitude <= 25) // Optimized 5m distance test
        {
            return false;
        }

        if (rigidbody)
        {
            rigidbody.position = position;
            rigidbody.rotation = rotation;
        }
        else
        {
            transform.position = position;
            transform.rotation = rotation;
        }
        return true;
    }

    public static RemotelyControlled Ensure(GameObject gameObject)
    {
        if (gameObject.TryGetComponent(out RemotelyControlled remotelyControlled))
        {
            return remotelyControlled;
        }
        if (gameObject.GetComponent<PipeSurfaceFloater>())
        {
            return gameObject.AddComponent<RemotelyControlledPipeFloater>();
        }
        return gameObject.AddComponent<RemotelyControlled>();
    }
}
