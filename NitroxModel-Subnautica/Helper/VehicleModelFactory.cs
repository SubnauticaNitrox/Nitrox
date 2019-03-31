using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    return new SeamothModel(packet.TechType, packet.ConstructedItemGuid, packet.Position, packet.Rotation, Optional<List<InteractiveChildObjectIdentifier>>.OfNullable(packet.InteractiveChildIdentifiers), Optional<string>.Empty(), packet.Name, packet.HSB, packet.Colours);
                case TechType.Exosuit:
                    return new ExosuitModel(packet.TechType, packet.ConstructedItemGuid, packet.Position, packet.Rotation, Optional<List<InteractiveChildObjectIdentifier>>.OfNullable(packet.InteractiveChildIdentifiers), Optional<string>.Empty(), packet.Name, packet.HSB, packet.Colours);
                case TechType.Cyclops:
                    return new CyclopsModel(packet.TechType, packet.ConstructedItemGuid, packet.Position, packet.Rotation, Optional<List<InteractiveChildObjectIdentifier>>.OfNullable(packet.InteractiveChildIdentifiers), Optional<string>.Empty(), packet.Name, packet.HSB, packet.Colours);
                case TechType.RocketBase:
                    return null;
                default:
                    throw new Exception("Could not build from: " + packet.TechType);

            }
        }
    }
}
