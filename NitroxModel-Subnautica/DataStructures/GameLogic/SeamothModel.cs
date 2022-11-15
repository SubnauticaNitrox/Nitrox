using System;
using System.Collections.Generic;
using BinaryPack.Attributes;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class SeamothModel : VehicleModel
    {
        [ProtoMember(1)]
        public bool LightOn { get; set; }

        [IgnoreConstructor]
        protected SeamothModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public SeamothModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
            : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            LightOn = true;
        }

        /// <remarks>Used for deserialization</remarks>
        public SeamothModel(
            NitroxTechType techType,
            NitroxId id,
            NitroxVector3 position,
            NitroxQuaternion rotation,
            ThreadSafeList<InteractiveChildObjectIdentifier> interactiveChildIdentifiers,
            Optional<NitroxId> dockingBayId,
            string name,
            NitroxVector3[] hsb,
            float health,
            bool lightOn)
            : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            LightOn = lightOn;
        }

        public override string ToString()
        {
            return $"[SeamothModel - {base.ToString()}, LightOn: {LightOn}]";
        }
    }
}
