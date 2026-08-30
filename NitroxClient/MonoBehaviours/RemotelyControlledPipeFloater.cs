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
        rigidbody.isKinematic = true;
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
