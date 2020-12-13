using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using ProtoBufNet;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class SeamothModel : VehicleModel
    {
        [ProtoMember(1)]
        public bool LightOn { get; set; }

        protected SeamothModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public SeamothModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
            : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            LightOn = true;
        }

        public override string ToString()
        {
            return $"[SeamothModel - {base.ToString()}, LightOn: {LightOn}]";
        }
    }
}
