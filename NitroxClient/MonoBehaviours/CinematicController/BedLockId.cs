using Nitrox.Model.DataStructures;

namespace NitroxClient.MonoBehaviours.CinematicController;

/// <summary>
/// A lock ID for beds, which have two sides and share one NitroxEntity.
/// </summary>
public static class BedLockId
{
    public static NitroxId Resolve(NitroxEntity entity, PlayerCinematicController controller)
    {
        if (!controller.TryGetComponentInParent(out Bed bed, true))
        {
            return entity.Id;
        }

        bool isRightSide = controller == bed.rightLieDownCinematicController || controller == bed.rightStandUpCinematicController;
        NitroxId leftId = entity.Id.Increment();
        return isRightSide ? leftId.Increment() : leftId;
    }
}
