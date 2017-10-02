using System.Runtime.Serialization;
using UnityEngine;

namespace NitroxModel.DataStructures.Surrogates
{
    public class QuaternionSurrogate : SerializationSurrogate<Quaternion>
    {
        protected override void GetObjectData(Quaternion quaternion, SerializationInfo info)
        {
            info.AddValue("w", quaternion.w);
            info.AddValue("x", quaternion.x);
            info.AddValue("y", quaternion.y);
            info.AddValue("z", quaternion.z);
        }

        protected override Quaternion SetObjectData(Quaternion quaternion, SerializationInfo info)
        {
            quaternion.w = info.GetSingle("w");
            quaternion.x = info.GetSingle("x");
            quaternion.y = info.GetSingle("y");
            quaternion.z = info.GetSingle("z");
            return quaternion;
        }
    }
}
