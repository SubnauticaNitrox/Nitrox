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
public class CyclopsModel : VehicleModel
{
    [JsonMemberTransition]
    public bool FloodLightsOn { get; set; }

    [JsonMemberTransition]
    public bool InternalLightsOn { get; set; }

    [JsonMemberTransition]
    public bool SilentRunningOn { get; set; }

    [JsonMemberTransition]
    public bool ShieldOn { get; set; }

    [JsonMemberTransition]
    public bool SonarOn { get; set; }

    [JsonMemberTransition]
    public bool EngineState { get; set; }

    [JsonMemberTransition]
    public CyclopsMotorMode.CyclopsMotorModes EngineMode { get; set; }

    public CyclopsModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
        : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
    {
        FloodLightsOn = true;
        InternalLightsOn = true;
        SilentRunningOn = false;
        ShieldOn = false;
        SonarOn = false;
        EngineState = false;
        EngineMode = CyclopsMotorMode.CyclopsMotorModes.Standard;
    }

    public override string ToString()
    {
        return $"[CyclopsModel - {base.ToString()}, FloodLightsOn: {FloodLightsOn}, InternalLightsOn: {InternalLightsOn}, SilentRunningOn: {SilentRunningOn}, ShieldOn: {ShieldOn}, SonarOn: {SonarOn}, EngineState: {EngineState}, EngineMode: {EngineMode}]";
    }
}
