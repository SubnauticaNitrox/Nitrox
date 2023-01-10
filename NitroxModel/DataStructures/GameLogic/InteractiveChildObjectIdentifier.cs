using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    public class InteractiveChildObjectIdentifier
    {
        [DataMember(Order = 1)]
        public NitroxId Id { get; set; }

        [DataMember(Order = 2)]
        public string GameObjectNamePath { get; set; }

        [IgnoreConstructor]
        protected InteractiveChildObjectIdentifier()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public InteractiveChildObjectIdentifier(NitroxId id, string gameObjectNamePath)
        {
            Id = id;
            GameObjectNamePath = gameObjectNamePath;
        }

        public override string ToString()
        {
            return $"[InteractiveChildObjectIdentifier - Id: {Id} GameObjectNamePath: {GameObjectNamePath}]";
        }
    }
}
