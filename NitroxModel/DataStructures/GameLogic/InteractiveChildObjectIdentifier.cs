using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class InteractiveChildObjectIdentifier
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxId Id { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual string GameObjectNamePath { get; set; }

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
            return "[InteractiveChildObjectIdentifier - Id: " + Id + " GameObjectNamePath: " + GameObjectNamePath + "]";
        }
    }
}
