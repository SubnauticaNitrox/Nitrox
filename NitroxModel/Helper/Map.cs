using System.Collections.Generic;
using NitroxInt3 = NitroxModel.DataStructures.Int3;
using NitroxTechType = NitroxModel.DataStructures.TechType;

namespace NitroxModel.Helper
{
    public abstract class Map
    {
        public static Map Main { get; set; }

        public abstract int BatchSize { get; }
        public abstract NitroxInt3 BatchDimensions { get; }
        public abstract NitroxInt3 DimensionsInMeters { get; }
        public abstract NitroxInt3 DimensionsInBatches { get; }
        public abstract NitroxInt3 BatchDimensionCenter { get; }
        public abstract List<NitroxTechType> GlobalRootTechTypes { get; }
        public abstract int ItemLevelOfDetail { get; }
    }
}
