using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Serialization;

namespace NitroxModel_Subnautica.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class SeamothModel : VehicleModel
{
    [JsonMemberTransition]
    public bool LightOn { get; set; }

    public SeamothModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
        : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
    {
        LightOn = true;
    }

    public override string ToString()
    {
        return $"[SeamothModel - {base.ToString()}, LightOn: {LightOn}]";
    }
}
