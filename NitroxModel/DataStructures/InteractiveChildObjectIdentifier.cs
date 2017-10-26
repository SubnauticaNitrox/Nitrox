using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class InteractiveChildObjectIdentifier
    {
        public String Guid { get; }
        public String GameObjectNamePath { get; }

        public InteractiveChildObjectIdentifier(String guid, String gameObjectNamePath)
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
