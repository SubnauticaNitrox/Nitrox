using System;
using System.Collections.Generic;
using BinaryPack.Attributes;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class NeptuneRocketModel : VehicleModel
    {
        [ProtoMember(1)]
        public int CurrentStage { get; set; }

        [ProtoMember(2)]
        public bool ElevatorUp { get; set; }

        [ProtoMember(3)]
        public ThreadSafeList<PreflightCheck> PreflightChecks { get; set; } = new();

        [IgnoreConstructor]
        protected NeptuneRocketModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public NeptuneRocketModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
            : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            CurrentStage = 0;
            ElevatorUp = false;
            PreflightChecks = new ThreadSafeList<PreflightCheck>();
        }

        /// <remarks>Used for deserialization</remarks>
        public NeptuneRocketModel(
            NitroxTechType techType,
            NitroxId id,
            NitroxVector3 position,
            NitroxQuaternion rotation,
            ThreadSafeList<InteractiveChildObjectIdentifier> interactiveChildIdentifiers,
            Optional<NitroxId> dockingBayId,
            string name,
            NitroxVector3[] hsb,
            float health,
            int currentStage,
            bool elevatorUp,
            ThreadSafeList<PreflightCheck> preflightChecks)
            : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            CurrentStage = currentStage;
            ElevatorUp = elevatorUp;
            PreflightChecks = preflightChecks;
        }

        public override string ToString()
        {
            return $"[NeptuneRocketModel - {base.ToString()}, CurrentStage: {CurrentStage}, ElevatorUp: {ElevatorUp}, Preflights: {PreflightChecks?.Count}]";
        }
    }
}
