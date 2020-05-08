using System.Collections.Generic;
using NitroxModel.Helper;
using NitroxTechType = NitroxModel.DataStructures.TechType;
using NitroxInt3 = NitroxModel.DataStructures.Int3;

namespace NitroxModel_Subnautica.Helper
{
    public class SubnauticaMap : Map
    {
        private const int BATCH_SIZE = 160;
        private const int SKYBOX_METERS_ABOVE_WATER = 160;

        public static readonly List<NitroxTechType> GLOBAL_ROOT_TECH_TYPES = new List<NitroxTechType>()
        {
            new NitroxTechType(TechType.Pipe.ToString()),
            new NitroxTechType(TechType.Constructor.ToString()),
            new NitroxTechType(TechType.Flare.ToString()),
            new NitroxTechType(TechType.Gravsphere.ToString()),
            new NitroxTechType(TechType.PipeSurfaceFloater.ToString()),
            new NitroxTechType(TechType.SmallStorage.ToString()),
            new NitroxTechType(TechType.CyclopsDecoy.ToString()),
            new NitroxTechType(TechType.LEDLight.ToString()),
            new NitroxTechType(TechType.Beacon.ToString())
        };

        public override int ItemLevelOfDetail => 3;
        public override int BatchSize => 160;
        public override NitroxInt3 BatchDimensions => new NitroxInt3(BatchSize, BatchSize, BatchSize);
        public override NitroxInt3 DimensionsInMeters => new NitroxInt3(4096, 3200, 4096);
        public override NitroxInt3 DimensionsInBatches => NitroxInt3.Ceil(DimensionsInMeters.ToVector3() / BATCH_SIZE);
        public override NitroxInt3 BatchDimensionCenter => new NitroxInt3(DimensionsInMeters.X / 2, DimensionsInMeters.Y - SKYBOX_METERS_ABOVE_WATER, DimensionsInMeters.Z / 2);
        public override List<NitroxTechType> GlobalRootTechTypes { get; } = GLOBAL_ROOT_TECH_TYPES;
    }
}
