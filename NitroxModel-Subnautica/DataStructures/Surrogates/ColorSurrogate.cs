using System.Runtime.Serialization;
using NitroxModel.DataStructures.Surrogates;
using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.Surrogates
{
    public class ColorSurrogate : SerializationSurrogate<Color>
    {
        protected override void GetObjectData(Color obj, SerializationInfo info)
        {
            info.AddValue("r", obj.r);
            info.AddValue("g", obj.g);
            info.AddValue("b", obj.b);
            info.AddValue("a", obj.a);
        }

        protected override Color SetObjectData(Color obj, SerializationInfo info)
        {
            obj.r = info.GetSingle("r");
            obj.g = info.GetSingle("g");
            obj.b = info.GetSingle("b");
            obj.a = info.GetSingle("a");
            return obj;
        }
    }
}
