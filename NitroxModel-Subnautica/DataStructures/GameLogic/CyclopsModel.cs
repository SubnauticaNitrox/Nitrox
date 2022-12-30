using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    public class CyclopsModel : VehicleModel
    {
        [DataMember(Order = 1)]
        public bool FloodLightsOn { get; set; }

        [DataMember(Order = 2)]
        public bool InternalLightsOn { get; set; }

        [DataMember(Order = 3)]
        public bool SilentRunningOn { get; set; }

        [DataMember(Order = 4)]
        public bool ShieldOn { get; set; }

        [DataMember(Order = 5)]
        public bool SonarOn { get; set; }

        [DataMember(Order = 6)]
        public bool EngineState { get; set; }

        [DataMember(Order = 7)]
        public CyclopsMotorMode.CyclopsMotorModes EngineMode { get; set; }

        [IgnoreConstructor]
        protected CyclopsModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public CyclopsModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, IEnumerable<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
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

        /// <remarks>Used for deserialization</remarks>
        public CyclopsModel(
            NitroxTechType techType,
            NitroxId id,
            NitroxVector3 position,
            NitroxQuaternion rotation,
            ThreadSafeList<InteractiveChildObjectIdentifier> interactiveChildIdentifiers,
            Optional<NitroxId> dockingBayId,
            string name,
            NitroxVector3[] hsb,
            float health,
            bool floodLightsOn,
            bool internalLightsOn,
            bool silentRunningOn,
            bool shieldOn,
            bool sonarOn,
            bool engineState,
            CyclopsMotorMode.CyclopsMotorModes engineMode)
            : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            FloodLightsOn = floodLightsOn;
            InternalLightsOn = internalLightsOn;
            SilentRunningOn = silentRunningOn;
            ShieldOn = shieldOn;
            SonarOn = sonarOn;
            EngineState = engineState;
            EngineMode = engineMode;
        }

        public override string ToString()
        {
            return $"[CyclopsModel - {base.ToString()}, FloodLightsOn: {FloodLightsOn}, InternalLightsOn: {InternalLightsOn}, SilentRunningOn: {SilentRunningOn}, ShieldOn: {ShieldOn}, SonarOn: {SonarOn}, EngineState: {EngineState}, EngineMode: {EngineMode}]";
        }
    }
}
