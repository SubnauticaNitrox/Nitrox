namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Ensures a deployed <see cref="PipeSurfaceFloater"/> is no longer remotely moved (it is supposed to be static).
/// Also ensures <see cref="RemotelyControlled.FixedUpdate"/> no longer sets isKinematic to false when deployed.
/// </summary>
public class RemotelyControlledPipeFloater : RemotelyControlled
{
    private PipeSurfaceFloater pipeSurfaceFloater;
    
    public new void Awake()
    {
        base.Awake();
        pipeSurfaceFloater = gameObject.GetComponent<PipeSurfaceFloater>();
    }

    public new void FixedUpdate()
    {
        if (!pipeSurfaceFloater.deployed)
        {
            base.FixedUpdate();
        }
    }
}
