namespace NitroxClient.Helpers;

public static class VFXConstructingHelper
{
    public static void EndGracefully(this VFXConstructing vfxConstructing)
    {
        // tell it we aren't done processing the OnSubComplete side-effects.
        vfxConstructing.isDone = false;

        // When vehicles are spawned they intentionally have a delay to remove the animation to give it a cinematic effect.
        // setting to 0 is not enough, value must be negative.
        vfxConstructing.delay = -1;

        // We only set the constructed amount and don't call any of the end methods.  For some reason, if the vfx doesn't 
        // end organically it can bug out the vehicle.  Users won't notice a difference.
        vfxConstructing.constructed = 1;

        // Height is only used to calculate splash effects (vehicle falling into the water).  This will disable it. 
        vfxConstructing.heightOffset = -999;

        // Force an update to lock in these changes before anything stateful happens.
        vfxConstructing.Update();
    }
}
