using NitroxClient.GameLogic;
using NitroxClient.Unity.Smoothing;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class RemotelyControlled : MonoBehaviour
{
    protected SmoothVector smoothPosition;
    protected SmoothRotation smoothRotation;

    private SwimBehaviour swimBehaviour;
    protected Rigidbody rigidbody;
    private WorldForces worldForces;

    private bool disabledWorldForces;

    public void Awake()
    {
        swimBehaviour = gameObject.GetComponent<SwimBehaviour>();
        rigidbody = gameObject.GetComponent<Rigidbody>();
        worldForces = gameObject.GetComponent<WorldForces>();

        bool followsSpline = swimBehaviour && swimBehaviour.enabled;
        if (rigidbody)
        {
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            if (followsSpline)
            {
                rigidbody.isKinematic = false;
            }
        }

        if (worldForces && !followsSpline)
        {
            disabledWorldForces = worldForces.enabled;
            worldForces.enabled = false;
        }

        smoothPosition = new(transform.position);
        smoothRotation = new(transform.rotation);
    }

    public void FixedUpdate()
    {
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

    public void OnDestroy()
    {
        // there might be other instances calling worldForces.enabled = false during its lifetime but we can't really detect those easily
        // so we just hope nothing major breaks
        if (worldForces && disabledWorldForces)
        {
            worldForces.enabled = true;
        }
    }

    public void UpdateOrientation(Vector3 position, Quaternion rotation)
    {
        float velocity = rigidbody ? rigidbody.velocity.magnitude : 0f;
        bool teleported = TeleportIfTooFar(position, rotation, GetTeleportThreshold(velocity));

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
        bool teleported = TeleportIfTooFar(currentPosition, currentRotation, GetTeleportThreshold(velocity));

        // SwimBehaviour and WalkBehaviour will act the exact same
        if (swimBehaviour && swimBehaviour.enabled)
        {
            float adjustedVelocity = velocity;

            if (!teleported)
            {
                float distance = Vector3.Distance(currentPosition, destination);

                // avoid too short paths
                if (distance > 0.1f)
                {
                    float localDistance = Vector3.Distance(transform.position, destination);

                    adjustedVelocity *= localDistance / distance;

                    adjustedVelocity = Mathf.Clamp(adjustedVelocity, velocity * 0.5f, velocity * 1.5f);
                }
            }

            // Adjust the target data and velocity
            swimBehaviour.originalTargetPosition = destination;
            swimBehaviour.originalTargetDirection = destinationDirection;
            swimBehaviour.originalVelocity = adjustedVelocity;

            // Trigger either SwimBehaviour.GoToInternal or WalkBehaviour.GoToInternal so they use their own way to pass the data to the SplineFollowing
            swimBehaviour.GoToInternal(destination, destinationDirection, adjustedVelocity);
        }
    }

    private bool TeleportIfTooFar(Vector3 position, Quaternion rotation, float teleportThreshold)
    {
        if ((transform.position - position).sqrMagnitude <= teleportThreshold * teleportThreshold)
        {
            return false;
        }

        if (rigidbody)
        {
            rigidbody.position = position;
            rigidbody.rotation = rotation;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            transform.position = position;
            transform.rotation = rotation;
        }
        return true;
    }

    private static float GetTeleportThreshold(float velocity)
    {
        return Mathf.Max(5f, velocity * 1.5f);
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
