using System;
using System.Collections.Generic;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    /*
     * Shim tech type model to bridge the gap between original subnautica and BZ
     */
    [ProtoContract]
    [Serializable]
    public class NitroxTechType
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        protected NitroxTechType()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public NitroxTechType(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            NitroxTechType type = obj as NitroxTechType;

            return !ReferenceEquals(type, null) &&
                   Name == type.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}
