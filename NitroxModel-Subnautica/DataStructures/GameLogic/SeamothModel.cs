using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class SeamothModel : VehicleModel
    {
        [ProtoMember(1)]
        public bool LightOn { get; set; }

        public SeamothModel()
        {

        }

        public SeamothModel(NitroxModel.DataStructures.TechType techType, string guid, Vector3 position, Quaternion rotation, Optional<List<InteractiveChildObjectIdentifier>> interactiveChildIdentifiers, Optional<string> dockingBayGuid, string name, Vector3[] hsb, Vector3[] colours) : base(techType, guid, position, rotation, interactiveChildIdentifiers, dockingBayGuid, name, hsb, colours)
        {
            LightOn = true;
        }
    }
}
