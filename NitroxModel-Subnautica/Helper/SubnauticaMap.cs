using System.Collections.Generic;
using NitroxModel.Helper;

namespace NitroxModel_Subnautica.Helper
{
    public class SubnauticaMap : Map
    {
        private const int BATCH_SIZE = 160;
        private const int SKYBOX_METERS_ABOVE_WATER = 160;

        public static readonly List<NitroxModel.DataStructures.TechType> GLOBAL_ROOT_TECH_TYPES = new List<NitroxModel.DataStructures.TechType>()
        {
            new NitroxModel.DataStructures.TechType(TechType.Pipe.ToString()),
            new NitroxModel.DataStructures.TechType(TechType.Constructor.ToString()),
            new NitroxModel.DataStructures.TechType(TechType.Flare.ToString()),
            new NitroxModel.DataStructures.TechType(TechType.Gravsphere.ToString()),
            new NitroxModel.DataStructures.TechType(TechType.PipeSurfaceFloater.ToString()),
            new NitroxModel.DataStructures.TechType(TechType.SmallStorage.ToString()),
            new NitroxModel.DataStructures.TechType(TechType.CyclopsDecoy.ToString()),
            new NitroxModel.DataStructures.TechType(TechType.LEDLight.ToString()),
            new NitroxModel.DataStructures.TechType(TechType.Beacon.ToString())
        };

        public override int ItemLevelOfDetail { get { return 3; } }
        public override int BatchSize { get { return 160; } }
        public override NitroxModel.DataStructures.Int3 BatchDimensions { get { return new NitroxModel.DataStructures.Int3(BatchSize, BatchSize, BatchSize); } }
        public override NitroxModel.DataStructures.Int3 DimensionsInMeters { get { return new NitroxModel.DataStructures.Int3(4096, 3200, 4096); } }
        public override NitroxModel.DataStructures.Int3 DimensionsInBatches { get { return NitroxModel.DataStructures.Int3.Ceil(DimensionsInMeters.ToVector3() / BATCH_SIZE); } }
        public override NitroxModel.DataStructures.Int3 BatchDimensionCenter
        {
            get
            {
                return new NitroxModel.DataStructures.Int3(DimensionsInMeters.X / 2,
                                                           DimensionsInMeters.Y - SKYBOX_METERS_ABOVE_WATER,
                                                           DimensionsInMeters.Z / 2);
            }
        }
        public override List<NitroxModel.DataStructures.TechType> GlobalRootTechTypes { get; } = GLOBAL_ROOT_TECH_TYPES;
    }
}
