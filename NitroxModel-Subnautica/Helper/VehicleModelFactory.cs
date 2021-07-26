using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic;

namespace NitroxModel_Subnautica.Helper
{
    public class VehicleModelFactory
    {
        public static VehicleModel BuildFrom(ConstructorBeginCrafting packet)
        {
            switch (packet.TechType.Enum())
            {
                case TechType.Seamoth:
                    return new SeamothModel(packet.TechType, packet.ConstructedItemId, packet.Position, packet.Rotation, packet.InteractiveChildIdentifiers, Optional.Empty, packet.Name, packet.HSB, packet.Colours, packet.Health);
                case TechType.Exosuit:
                    return new ExosuitModel(packet.TechType, packet.ConstructedItemId, packet.Position, packet.Rotation, packet.InteractiveChildIdentifiers, Optional.Empty, packet.Name, packet.HSB, packet.Colours, packet.Health);
                case TechType.Cyclops:
                    return new CyclopsModel(packet.TechType, packet.ConstructedItemId, packet.Position, packet.Rotation, packet.InteractiveChildIdentifiers, Optional.Empty, packet.Name, packet.HSB, packet.Colours, packet.Health);
                case TechType.RocketBase:
                    return null;
                default:
                    throw new Exception("Could not build from: " + packet.TechType);

            }
        }
    }
}
