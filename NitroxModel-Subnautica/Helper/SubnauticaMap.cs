using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;

namespace NitroxModel_Subnautica.Helper
{
    /// <summary>
    ///     Static information about the game world loaded by Subnautica that isn't (and shouldn't) be retrievable from the game directly. 
    /// </summary>
    public class SubnauticaMap : IMap
    {
        private const int BATCH_SIZE = 160;
        private const int SKYBOX_METERS_ABOVE_WATER = 160;

        /// <summary>
        ///     TechType can't be introspected at runtime in RELEASE mode because its reflection info is elided.
        /// </summary>
        public static readonly List<NitroxTechType> GLOBAL_ROOT_TECH_TYPES = new List<NitroxTechType>
        {
            new NitroxTechType(nameof(TechType.Pipe)),
            new NitroxTechType(nameof(TechType.Constructor)),
            new NitroxTechType(nameof(TechType.Flare)),
            new NitroxTechType(nameof(TechType.Gravsphere)),
            new NitroxTechType(nameof(TechType.PipeSurfaceFloater)),
            new NitroxTechType(nameof(TechType.SmallStorage)),
            new NitroxTechType(nameof(TechType.CyclopsDecoy)),
            new NitroxTechType(nameof(TechType.LEDLight)),
            new NitroxTechType(nameof(TechType.Beacon))
        };

        public int ItemLevelOfDetail => 3;
        public int BatchSize => 160;
        public NitroxInt3 BatchDimensions => new NitroxInt3(BatchSize, BatchSize, BatchSize);
        public NitroxInt3 DimensionsInMeters => new NitroxInt3(4096, 3200, 4096);
        public NitroxInt3 DimensionsInBatches => NitroxInt3.Ceil(DimensionsInMeters / BATCH_SIZE);
        public NitroxInt3 BatchDimensionCenter => new NitroxInt3(DimensionsInMeters.X / 2, DimensionsInMeters.Y - SKYBOX_METERS_ABOVE_WATER, DimensionsInMeters.Z / 2);
        public List<NitroxTechType> GlobalRootTechTypes { get; } = GLOBAL_ROOT_TECH_TYPES;
    }
}
