using System;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InteractiveChildObjectIdentifier
    {
        public NitroxId Id { get; }
        public string GameObjectNamePath { get; }

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
