using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class CyclopsModel : VehicleModel
    {
        [ProtoMember(1)]
        public bool FloodLightsOn { get; set; }

        [ProtoMember(2)]
        public bool InternalLightsOn { get; set; }

        [ProtoMember(3)]
        public bool SilentRunningOn { get; set; }

        [ProtoMember(4)]
        public bool ShieldOn { get; set; }

        [ProtoMember(5)]
        public bool SonarOn { get; set; }

        [ProtoMember(6)]
        public bool EngineState { get; set; }

        [ProtoMember(7)]
        public CyclopsMotorMode.CyclopsMotorModes EngineMode { get; set; }

        protected CyclopsModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

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
}
