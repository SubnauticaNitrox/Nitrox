using System.Runtime.Serialization;
using NitroxModel.DataStructures.Surrogates;
using Color = NitroxModel.DataStructures.Color;

namespace NitroxModel_Subnautica.DataStructures.Surrogates
{
    public class ColorSurrogate : SerializationSurrogate<UnityEngine.Color>
    {
        protected override void GetObjectData(UnityEngine.Color color, SerializationInfo info)
        {
            info.AddValue("r", color.r);
            info.AddValue("g", color.g);
            info.AddValue("b", color.b);
            info.AddValue("a", color.a);
        }

        protected override UnityEngine.Color SetObjectData(UnityEngine.Color color, SerializationInfo info)
        {
            color.r = info.GetSingle("r");
            color.g = info.GetSingle("g");
            color.b = info.GetSingle("b");
            color.a = info.GetSingle("a");
            return color;
        }
    }
}
