using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Helper
{
    public interface IMap
    {
        public int BatchSize { get; }
        /// <summary>
        /// AKA LargeWorldStreamer.blocksPerBatch
        /// </summary>
        public NitroxInt3 BatchDimensions { get; }
        public NitroxInt3 DimensionsInMeters { get; }
        public NitroxInt3 DimensionsInBatches { get; }
        public NitroxInt3 BatchDimensionCenter { get; }
        public List<NitroxTechType> GlobalRootTechTypes { get; }
        public int ItemLevelOfDetail { get; }
    }
}
