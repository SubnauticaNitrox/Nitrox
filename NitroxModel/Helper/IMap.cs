using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Helper
{
    public interface IMap
    {
        public int BatchSize { get; }
        public NitroxInt3 BatchDimensions { get; }
        public NitroxInt3 DimensionsInMeters { get; }
        public NitroxInt3 DimensionsInBatches { get; }
        public NitroxInt3 BatchDimensionCenter { get; }
        public List<NitroxTechType> GlobalRootTechTypes { get; }
        public int ItemLevelOfDetail { get; }
    }
}
