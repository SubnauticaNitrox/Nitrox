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

        public ExosuitModel()
        {

        }

        public ExosuitModel(NitroxModel.DataStructures.TechType techType, NitroxId id, Vector3 position, Quaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, Vector3[] hsb, Vector3[] colours, float health) : base (techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, colours, health)
        {
            LeftArmId = new NitroxId();
            RightArmId = new NitroxId();
        }
    }
}

