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
public class ExosuitModel : VehicleModel
{
    [JsonMemberTransition]
    public NitroxId LeftArmId { get; }

    [JsonMemberTransition]
    public NitroxId RightArmId { get; }

    public ExosuitModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
        : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
    {
        LeftArmId = new NitroxId();
        RightArmId = new NitroxId();
    }

    public override string ToString()
    {
        return $"[ExosuitModel - {base.ToString()}, LeftArmId: {LeftArmId}, RightArmId: {RightArmId}]";
    }
}
