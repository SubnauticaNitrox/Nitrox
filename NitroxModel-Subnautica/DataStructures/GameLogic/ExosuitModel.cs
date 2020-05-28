using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using System;
using System.Collections.Generic;
using UnityEngine;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class ExosuitModel : VehicleModel
    {
        [ProtoMember(10)]
        public NitroxId LeftArmId { get; }

        [ProtoMember(11)]
        public NitroxId RightArmId { get; }

        protected ExosuitModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public ExosuitModel(NitroxModel.DataStructures.GameLogic.NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb,  float health) : base (techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            LeftArmId = new NitroxId();
            RightArmId = new NitroxId();
        }
    }
}

