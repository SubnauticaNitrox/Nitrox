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
    public class ExosuitModel : VehicleModel
    {
        [ProtoMember(1)]
        public NitroxId LeftArmId { get; }

        [ProtoMember(2)]
        public NitroxId RightArmId { get; }

        protected ExosuitModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

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
}

