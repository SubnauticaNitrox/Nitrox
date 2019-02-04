using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class VehicleFactory
    {
        public static VehicleModel CreateFrom(VehicleModel vehicleModel)
        {
            switch (vehicleModel.TechType)
            {
                case TechType.Seamoth:
                    return new VehicleModel(vehicleModel.TechType, vehicleModel.Guid, vehicleModel.Position, vehicleModel.Rotation, vehicleModel.InteractiveChildIdentifiers, Optional<string>.Empty(), vehicleModel.Name, vehicleModel.HSB, vehicleModel.Colours);
                case TechType.Exosuit:
                    return new ExosuitModel(vehicleModel.TechType, vehicleModel.Guid, vehicleModel.Position, vehicleModel.Rotation, vehicleModel.InteractiveChildIdentifiers, Optional<string>.Empty(), vehicleModel.Name, vehicleModel.HSB, vehicleModel.Colours);
                case TechType.Cyclops:
                    return new VehicleModel(vehicleModel.TechType, vehicleModel.Guid, vehicleModel.Position, vehicleModel.Rotation, vehicleModel.InteractiveChildIdentifiers, Optional<string>.Empty(), vehicleModel.Name, vehicleModel.HSB, vehicleModel.Colours);
                case TechType.RocketBase:
                    return null;
                default:
                    throw new Exception("Could not create Vehicle from " + vehicleModel.TechType);

            }
        }
    }
}
