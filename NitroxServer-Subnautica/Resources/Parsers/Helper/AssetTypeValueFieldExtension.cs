using AssetsTools.NET;
using NitroxModel.DataStructures.Unity;
using UnityEngine;

namespace NitroxServer_Subnautica.Resources.Parsers.Helper;

public static class AssetTypeValueFieldExtension
{
    public static Vector3 ToVector3(this AssetTypeValueField valueField)
    {
        return new Vector3(valueField["x"].AsFloat, valueField["y"].AsFloat, valueField["z"].AsFloat);
    }
    
    public static NitroxVector3 ToNitroxVector3(this AssetTypeValueField valueField)
    {
        return new NitroxVector3(valueField["x"].AsFloat, valueField["y"].AsFloat, valueField["z"].AsFloat);
    }

    public static NitroxQuaternion ToNitroxQuaternion(this AssetTypeValueField valueField)
    {
        return new NitroxQuaternion(valueField["x"].AsFloat, valueField["y"].AsFloat, valueField["z"].AsFloat, valueField["w"].AsFloat);
    }
}
