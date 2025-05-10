using NitroxModel.DataStructures;

namespace NitroxModel.Helper;

/// <summary>
///     Static information about the game world loaded by Subnautica that isn't (and shouldn't) be retrievable from the game directly.
/// </summary>
public static class SubnauticaMap
{
    private const int BATCH_SIZE = 160;
    private const int SKYBOX_METERS_ABOVE_WATER = 160;

    public static int ItemLevelOfDetail => 3;
    public static int BatchSize => 160;
    public static NitroxInt3 BatchDimensions => new(BatchSize, BatchSize, BatchSize);
    public static NitroxInt3 DimensionsInMeters => new(4096, 3200, 4096);
    public static NitroxInt3 DimensionsInBatches => NitroxInt3.Ceil(DimensionsInMeters / BATCH_SIZE);
    public static NitroxInt3 BatchDimensionCenter => new(DimensionsInMeters.X / 2, DimensionsInMeters.Y - SKYBOX_METERS_ABOVE_WATER, DimensionsInMeters.Z / 2);
}
