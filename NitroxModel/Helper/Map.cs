namespace NitroxModel.Helper
{
    public class Map
    {
        public static readonly Int3 WORLD_SIZE = new Int3(4096, 3200, 4096);
        public static readonly int BATCH_SIZE = 160;
        public static readonly Int3 CELLS_PER_BATCH = new Int3(10, 10, 10);
        public static readonly Int3 BATCH_DIMENSIONS = new Int3(BATCH_SIZE, BATCH_SIZE, BATCH_SIZE);

        public static readonly Int3 DIMENSIONS = WORLD_SIZE / BATCH_DIMENSIONS;
        public static readonly Int3 DIMENSIONS_IN_BATCHES = DIMENSIONS * BATCH_DIMENSIONS;
        public static readonly Int3 DIMENSIONS_IN_CELLS = DIMENSIONS_IN_BATCHES * CELLS_PER_BATCH;

        public static readonly int SKYBOX_METERS_ABOVE_WATER = 160;

        public static readonly Int3 BATCH_DIMENSION_CENTERING = new Int3(WORLD_SIZE.x / 2,
                                                                         WORLD_SIZE.y - SKYBOX_METERS_ABOVE_WATER,
                                                                         WORLD_SIZE.z / 2);
    }
}
