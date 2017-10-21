namespace NitroxServer.GameLogic
{
    public static class EntityCellHelper
    {
        public static readonly Int3 CELLS_PER_BATCH = new Int3(10, 10, 10);

        public static Int3 GetAbsoluteCellId(Int3 batchId, Int3 cellId)
        {
            return (batchId * CELLS_PER_BATCH) + cellId;
        }
    }
}
