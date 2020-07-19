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
        public NitroxId ConstructorId { get; }

        [ProtoMember(2)]
        public int CurrentStage { get; set; }

        [ProtoMember(3)]
        public bool ElevatorUp { get; set; }

        protected NeptuneRocketModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public NeptuneRocketModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
            : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            CurrentStage = 0;
            ElevatorUp = false;
            ConstructorId = new NitroxId();
        }

        public override string ToString()
        {
            return $"[NeptuneRocketModel - {base.ToString()}, ConstructorId: {ConstructorId}, CurrentStage: {CurrentStage}, ElevatorUp: {ElevatorUp}]";
        }
    }
}
