namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Ensures a surfaced <see cref="PipeSurfaceFloater"/> is no longer movable (it is supposed to be static).
/// </summary>
public class RemotelyControlledPipeFloater : RemotelyControlled
{
    private bool positioned;

    public void SetPositioned()
    {
        positioned = true;
        // We set the floater to kinematic so it's unaffected by the local world
        rigidbody.isKinematic = true;
        // interpolation to none prevents some little jittering
        rigidbody.interpolation = UnityEngine.RigidbodyInterpolation.None;
    }

    public new void FixedUpdate()
    {
        if (positioned)
        {
            rigidbody.position = smoothPosition.Target;
            rigidbody.rotation = smoothRotation.Target;
        }
        else
        {
            base.FixedUpdate();
        }
    }
}
