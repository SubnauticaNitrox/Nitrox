using System.Collections.Generic;
using NitroxModel.DataStructures;

namespace NitroxModel.Helper
{
    public abstract class Map
    {
        public static Map Main { get; set; }

        public abstract int BatchSize { get; }
        public abstract Int3 BatchDimensions { get; }
        public abstract Int3 DimensionsInMeters { get; }
        public abstract Int3 DimensionsInBatches { get; }
        public abstract Int3 BatchDimensionCenter { get; }
        public abstract List<TechType> GlobalRootTechTypes { get; }
        public abstract int ItemLevelOfDetail { get; }
    }
}
