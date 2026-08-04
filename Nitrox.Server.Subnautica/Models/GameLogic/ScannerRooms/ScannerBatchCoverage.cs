using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal static class ScannerBatchCoverage
{
    public static IReadOnlyList<NitroxInt3> EnumerateIntersectingBatches(NitroxVector3 center, float radius)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(radius);

        NitroxInt3 dimensions = SubnauticaMap.DimensionsInBatches;
        NitroxInt3 mapOffset = SubnauticaMap.BatchDimensionCenter;
        int batchSize = SubnauticaMap.BatchSize;

        int minX = Math.Clamp((int)MathF.Floor((center.X - radius + mapOffset.X) / batchSize), 0, dimensions.X - 1);
        int minY = Math.Clamp((int)MathF.Floor((center.Y - radius + mapOffset.Y) / batchSize), 0, dimensions.Y - 1);
        int minZ = Math.Clamp((int)MathF.Floor((center.Z - radius + mapOffset.Z) / batchSize), 0, dimensions.Z - 1);
        int maxX = Math.Clamp((int)MathF.Floor((center.X + radius + mapOffset.X) / batchSize), 0, dimensions.X - 1);
        int maxY = Math.Clamp((int)MathF.Floor((center.Y + radius + mapOffset.Y) / batchSize), 0, dimensions.Y - 1);
        int maxZ = Math.Clamp((int)MathF.Floor((center.Z + radius + mapOffset.Z) / batchSize), 0, dimensions.Z - 1);

        List<NitroxInt3> batches = [];
        float radiusSquared = radius * radius;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    NitroxInt3 batch = new(x, y, z);
                    if (SquaredDistanceToBatch(center, batch) <= radiusSquared)
                    {
                        batches.Add(batch);
                    }
                }
            }
        }

        return batches;
    }

    private static float SquaredDistanceToBatch(NitroxVector3 point, NitroxInt3 batch)
    {
        NitroxInt3 minimum = batch * SubnauticaMap.BatchSize - SubnauticaMap.BatchDimensionCenter;
        NitroxInt3 maximum = minimum + SubnauticaMap.BatchDimensions;

        float dx = DistanceToInterval(point.X, minimum.X, maximum.X);
        float dy = DistanceToInterval(point.Y, minimum.Y, maximum.Y);
        float dz = DistanceToInterval(point.Z, minimum.Z, maximum.Z);
        return dx * dx + dy * dy + dz * dz;
    }

    private static float DistanceToInterval(float value, float minimum, float maximum)
    {
        if (value < minimum)
        {
            return minimum - value;
        }
        if (value > maximum)
        {
            return value - maximum;
        }
        return 0;
    }
}
