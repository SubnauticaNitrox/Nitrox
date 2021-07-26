using System;
using System.Collections.Generic;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    /*
     * Shim tech type model to bridge the gap between original subnautica and BZ
     */
    [ProtoContract]
    [Serializable]
    public class TechType
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        public TechType()
        {
            // Serialization Constructor
        }

        public TechType(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            TechType type = obj as TechType;

            return !ReferenceEquals(type, null) &&
                   Name == type.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}
