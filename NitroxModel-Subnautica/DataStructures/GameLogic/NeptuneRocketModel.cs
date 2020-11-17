using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures;
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
        public ThreadSafeCollection<PreflightCheck> PreflightChecks { get; set; }

        protected NeptuneRocketModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public NeptuneRocketModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
            : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            CurrentStage = 0;
            ElevatorUp = false;
            PreflightChecks = new ThreadSafeCollection<PreflightCheck>();
        }

        public override string ToString()
        {
            return $"[NeptuneRocketModel - {base.ToString()}, CurrentStage: {CurrentStage}, ElevatorUp: {ElevatorUp}, Preflights: {PreflightChecks?.Count}]";
        }
    }
}
