using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class ExosuitModel : VehicleModel
    {
        [ProtoMember(1)]
        public string LeftArmGuid { get; }

        [ProtoMember(2)]
        public string RightArmGuid { get; }

        public ExosuitModel(TechType techType, string guid, Vector3 position, Quaternion rotation, Optional<List<InteractiveChildObjectIdentifier>> interactiveChildIdentifiers, Optional<string> dockingBayGuid, string name, Vector3[] hsb, Vector3[] colours) : base(techType, guid, position, rotation, interactiveChildIdentifiers, dockingBayGuid, name, hsb, colours)
        {
            LeftArmGuid = System.Guid.NewGuid().ToString();
            RightArmGuid = System.Guid.NewGuid().ToString();
        }
    }
}

