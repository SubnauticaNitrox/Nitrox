using System.Collections.Generic;
using NitroxModel.Helper;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Helper
{
    public class SubnauticaMap : Map
    {
        private const int BATCH_SIZE = 160;
        private const int SKYBOX_METERS_ABOVE_WATER = 160;

        public static readonly List<DTO.TechType> GLOBAL_ROOT_TECH_TYPES = new List<DTO.TechType>
        {
            new DTO.TechType(TechType.Pipe.ToString()),
            new DTO.TechType(TechType.Constructor.ToString()),
            new DTO.TechType(TechType.Flare.ToString()),
            new DTO.TechType(TechType.Gravsphere.ToString()),
            new DTO.TechType(TechType.PipeSurfaceFloater.ToString()),
            new DTO.TechType(TechType.SmallStorage.ToString()),
            new DTO.TechType(TechType.CyclopsDecoy.ToString()),
            new DTO.TechType(TechType.LEDLight.ToString()),
            new DTO.TechType(TechType.Beacon.ToString())
        };

        public override int ItemLevelOfDetail => 3;
        public override int BatchSize => 160;
        public override DTO.Int3 BatchDimensions => new DTO.Int3(BatchSize, BatchSize, BatchSize);
        public override DTO.Int3 DimensionsInMeters => new DTO.Int3(4096, 3200, 4096);
        public override DTO.Int3 DimensionsInBatches => DTO.Int3.Ceil(DimensionsInMeters.ToVector3() / BATCH_SIZE);

        public override DTO.Int3 BatchDimensionCenter =>
            new DTO.Int3(DimensionsInMeters.X / 2,
                                                DimensionsInMeters.Y - SKYBOX_METERS_ABOVE_WATER,
                                                DimensionsInMeters.Z / 2);

        public override List<DTO.TechType> GlobalRootTechTypes { get; } = GLOBAL_ROOT_TECH_TYPES;
    }
}
