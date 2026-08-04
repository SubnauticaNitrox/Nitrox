using System;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

public static class ScannerRoomQueryParameters
{
    public const float MINIMUM_RANGE = 300f;
    public const float MAXIMUM_RANGE = 500f;
    public const float RANGE_INCREMENT = 50f;

    public static float NormalizeRange(float reportedRange)
    {
        if (float.IsNaN(reportedRange) || float.IsInfinity(reportedRange))
        {
            return MINIMUM_RANGE;
        }

        float clamped = Math.Min(Math.Max(reportedRange, MINIMUM_RANGE), MAXIMUM_RANGE);
        return MINIMUM_RANGE + (float)Math.Floor((clamped - MINIMUM_RANGE) / RANGE_INCREMENT) * RANGE_INCREMENT;
    }

    public static NitroxTechType? NormalizeSelection(NitroxTechType? selectedTechType) =>
        selectedTechType?.Equals(NitroxTechType.None) == true ? null : selectedTechType;
}
