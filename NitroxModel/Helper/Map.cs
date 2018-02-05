using System;

namespace NitroxModel.Helper
{
    public class Map
    {
        public static readonly int BATCH_SIZE = 160;

        public static readonly Int3 BATCH_DIMENSIONS = new Int3(BATCH_SIZE, BATCH_SIZE, BATCH_SIZE);
        
        public static readonly Int3 DIMENSIONS_IN_METERS = new Int3(4096, 3200, 4096);

        public static readonly Int3 DIMENSIONS_IN_BATCHES = new Int3((int)Math.Ceiling(DIMENSIONS_IN_METERS.x / (double)BATCH_DIMENSIONS.x),
                                                                     (int)Math.Ceiling(DIMENSIONS_IN_METERS.y / (double)BATCH_DIMENSIONS.y),
                                                                     (int)Math.Ceiling(DIMENSIONS_IN_METERS.z / (double)BATCH_DIMENSIONS.z));

        public static readonly int SKYBOX_METERS_ABOVE_WATER = 160;

        // This is the same translation as found in LargeWorldStreamer.land.transform.
        public static readonly Int3 BATCH_DIMENSION_CENTERING = new Int3(DIMENSIONS_IN_METERS.x / 2,
                                                                         DIMENSIONS_IN_METERS.y - SKYBOX_METERS_ABOVE_WATER,
                                                                         DIMENSIONS_IN_METERS.z / 2);
    }
}
