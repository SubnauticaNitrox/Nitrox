﻿using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class InteractiveChildObjectIdentifier
    {
        public String Guid { get; private set; }
        public String GameObjectNamePath { get; private set; }

        public InteractiveChildObjectIdentifier(String guid, String gameObjectNamePath)
        {
            this.Guid = guid;
            this.GameObjectNamePath = gameObjectNamePath;
        }

        public override string ToString()
        {
            return "[DependantObjectIdentifier - Guid: " + Guid + " GameObjectNamePath: " + GameObjectNamePath + "]";
        }
    }
}
