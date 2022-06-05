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
public class NeptuneRocketModel : VehicleModel
{
    [JsonMemberTransition]
    public int CurrentStage { get; set; }

    [JsonMemberTransition]
    public bool ElevatorUp { get; set; }

    [JsonMemberTransition]
    public ThreadSafeList<PreflightCheck> PreflightChecks { get; set; }

    public NeptuneRocketModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
        : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
    {
        CurrentStage = 0;
        ElevatorUp = false;
        PreflightChecks = new ThreadSafeList<PreflightCheck>();
    }

    public override string ToString()
    {
        return $"[NeptuneRocketModel - {base.ToString()}, CurrentStage: {CurrentStage}, ElevatorUp: {ElevatorUp}, Preflights: {PreflightChecks?.Count}]";
    }
}
