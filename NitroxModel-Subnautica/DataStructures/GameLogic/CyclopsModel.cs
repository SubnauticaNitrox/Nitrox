using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using UnityEngine;

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

        public CyclopsModel()
        {

        }

        public CyclopsModel(NitroxModel.DataStructures.TechType techType, NitroxId id, Vector3 position, Quaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, Vector3[] hsb, Vector3[] colours, float health) : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, colours, health)
        {
            FloodLightsOn = true;
            InternalLightsOn = true;
            SilentRunningOn = false;
            ShieldOn = false;
            SonarOn = false;
            EngineState = false;
            EngineMode = CyclopsMotorMode.CyclopsMotorModes.Standard;
        }
    }
}
