using System.Collections.Generic;

namespace NitroxModel.Helper
{
    public class Map
    {
        public const int BATCH_SIZE = 160;

        public static readonly Int3 BATCH_DIMENSIONS = new Int3(BATCH_SIZE, BATCH_SIZE, BATCH_SIZE);

        public static readonly Int3 DIMENSIONS_IN_METERS = new Int3(4096, 3200, 4096);

        public static readonly Int3 DIMENSIONS_IN_BATCHES = Int3.Ceil(DIMENSIONS_IN_METERS.ToVector3() / BATCH_SIZE);

        public const int SKYBOX_METERS_ABOVE_WATER = 160;

        // This is the same translation as found in LargeWorldStreamer.land.transform.
        public static readonly Int3 BATCH_DIMENSION_CENTERING = new Int3(DIMENSIONS_IN_METERS.x / 2,
                                                                         DIMENSIONS_IN_METERS.y - SKYBOX_METERS_ABOVE_WATER,
                                                                         DIMENSIONS_IN_METERS.z / 2);

        public static readonly int ITEM_LEVEL_OF_DETAIL = 3;

        public static readonly List<TechType> GLOBAL_ROOT_TECH_TYPES = new List<TechType>()
        {
            TechType.Pipe,
            TechType.Beacon
        };
    }
}
