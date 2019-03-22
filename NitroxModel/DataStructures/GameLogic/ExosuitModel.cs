using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using ProtoBufNet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class ExosuitModel : VehicleModel
    {
        [ProtoMember(10)]
        public string LeftArmGuid { get; }

        [ProtoMember(11)]
        public string RightArmGuid { get; }

        public ExosuitModel()
        {

        }
        public ExosuitModel(TechType techType, string guid, Vector3 position, Quaternion rotation, Optional<List<InteractiveChildObjectIdentifier>> interactiveChildIdentifiers, Optional<string> dockingBayGuid, string name, Vector3[] hsb, Vector3[] colours) : base (techType, guid, position, rotation, interactiveChildIdentifiers, dockingBayGuid, name, hsb, colours)
        {
            LeftArmGuid = System.Guid.NewGuid().ToString();
            RightArmGuid = System.Guid.NewGuid().ToString();
        }
    }
}

