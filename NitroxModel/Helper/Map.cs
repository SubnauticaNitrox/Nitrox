using System.Collections.Generic;
using M = NitroxModel.DataStructures;

namespace NitroxModel.Helper
{
    public abstract class Map
    {
        public static Map Main { get; set; }

        public abstract int BatchSize { get; }
        public abstract M.Int3 BatchDimensions { get; }
        public abstract M.Int3 DimensionsInMeters { get; }
        public abstract M.Int3 DimensionsInBatches { get; }
        public abstract M.Int3 BatchDimensionCenter { get; }
        public abstract List<M.TechType> GlobalRootTechTypes { get; }
        public abstract int ItemLevelOfDetail { get; }
    }
}
