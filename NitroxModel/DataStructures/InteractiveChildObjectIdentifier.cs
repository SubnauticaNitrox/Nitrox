using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class InteractiveChildObjectIdentifier
    {
        public String Guid { get; private set; }
        public String GameObjectNamePath { get; private set; }

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
