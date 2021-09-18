using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic;

namespace NitroxModel_Subnautica.Helper
{
    public static class VehicleModelFactory
    {
        public static VehicleModel BuildFrom(NitroxTechType techType, NitroxId constructedItemId, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
        {
            switch (techType.ToUnity())
            {
                case TechType.Seamoth:
                    return new SeamothModel(techType, constructedItemId, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health);
                case TechType.Exosuit:
                    return new ExosuitModel(techType, constructedItemId, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health);
                case TechType.Cyclops:
                    return new CyclopsModel(techType, constructedItemId, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health);
                case TechType.RocketBase:
                    return new NeptuneRocketModel(techType, constructedItemId, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health);
                default:
                    throw new Exception($"Could not build from: {techType}");
            }
        }
    }
}
