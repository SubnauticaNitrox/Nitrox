using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class VehicleCreatorFactory
    {
        public static VehicleModel CreateFrom(VehicleModel vehicleModels)
        {
            switch (vehicleModels.TechType)
            {
                case TechType.Seamoth:
                    return new VehicleModel(vehicleModels.TechType, vehicleModels.Guid, vehicleModels.Position, vehicleModels.Rotation, vehicleModels.InteractiveChildIdentifiers, Optional<string>.Empty(), vehicleModels.Name, vehicleModels.HSB, vehicleModels.Colours);
                case TechType.Exosuit:
                    return new ExosuitModel(vehicleModels.TechType, vehicleModels.Guid, vehicleModels.Position, vehicleModels.Rotation, vehicleModels.InteractiveChildIdentifiers, Optional<string>.Empty(), vehicleModels.Name, vehicleModels.HSB, vehicleModels.Colours);
                case TechType.Cyclops:
                    return new VehicleModel(vehicleModels.TechType, vehicleModels.Guid, vehicleModels.Position, vehicleModels.Rotation, vehicleModels.InteractiveChildIdentifiers, Optional<string>.Empty(), vehicleModels.Name, vehicleModels.HSB, vehicleModels.Colours);
                case TechType.RocketBase:
                    return null;
                default:
                    throw new Exception
                        ("Unrecognized TechType value.");
            }
        }
    }
}
