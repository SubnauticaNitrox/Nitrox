using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class InteractiveChildObjectIdentifier
    {
        public string Guid { get; }
        public string GameObjectNamePath { get; }

        public InteractiveChildObjectIdentifier(string guid, string gameObjectNamePath)
        {
            Guid = guid;
            GameObjectNamePath = gameObjectNamePath;
        }

        public override string ToString()
        {
            return "[InteractiveChildObjectIdentifier - Guid: " + Guid + " GameObjectNamePath: " + GameObjectNamePath + "]";
        }
    }
}
