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
        [ProtoMember(1)]
        public NitroxId LeftArmId { get; }

        [ProtoMember(2)]
        public NitroxId RightArmId { get; }

        public ExosuitModel()
        {
            //For serialization purposes
        }

        public ExosuitModel(NitroxModel.DataStructures.TechType techType, NitroxId id, Vector3 position, Quaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, Vector3[] hsb,  float health) 
            : base (techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            LeftArmId = new NitroxId();
            RightArmId = new NitroxId();
        }

        public override string ToString()
        {
            return $"[ExosuitModel : {base.ToString()}, LeftArmId: {LeftArmId}, RightArmId: {RightArmId}]";
        }
    }
}

