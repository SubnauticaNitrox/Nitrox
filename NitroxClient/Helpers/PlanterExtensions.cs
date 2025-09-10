using System.Diagnostics.CodeAnalysis;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;

namespace NitroxClient.Helpers;

public static class PlanterExtensions
{
    /// <summary>
    /// Returns <c>true</c> with <paramref name="ownerNitroxId"/> being the NitroxId of the entity responsible for the planter if it can be found.
    /// Else returns <c>false</c>.
    /// </summary>
    public static bool TryGetOwnerNitroxId(this Planter planter, [NotNullWhen(true)] out NitroxId? ownerNitroxId)
    {
        if (!planter)
        {
            Log.Error("Tried getting owner NitroxId of null planter");
            ownerNitroxId = null;
            return false;
        }

        // Multiple cases:
        // 1. outdoor planter, it is responsible for itself
        if (!planter.isIndoor)
        {
            return planter.TryGetNitroxId(out ownerNitroxId);
        }

        switch (planter.environment)
        {
            // 2. indoor planter, not in waterpark, the base is responsible
            case Planter.PlantEnvironment.Air:
                if (planter.TryGetComponentInParent(out Base parentBase))
                {
                    return parentBase.TryGetNitroxId(out ownerNitroxId);
                }
                break;

            // 3. indoor planter, in waterpark, the water park is responsible
            case Planter.PlantEnvironment.Water:
                if (planter.TryGetComponentInParent(out WaterPark waterPark))
                {
                    return waterPark.TryGetNitroxId(out ownerNitroxId);
                }
                break;
        }

        ownerNitroxId = null;
        return false;
    }
}
