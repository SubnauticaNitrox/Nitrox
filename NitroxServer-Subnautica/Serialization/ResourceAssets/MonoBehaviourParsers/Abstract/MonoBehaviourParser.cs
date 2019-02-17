using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetsTools.NET.Extra;

namespace NitroxServer_Subnautica.Serialization.ResourceAssets.MonoBehaviourParsers.Abstract
{
    public abstract class MonoBehaviourParser
    {
        public abstract void Parse(AssetsFileInstance instance, long gameObjectPathId, ResourceAssets resourceAssets);
    }
}
