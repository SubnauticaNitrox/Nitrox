using System.Runtime.Serialization;
using UnityEngine;

namespace NitroxModel.DataStructures.Surrogates
{
    public class Vector3Surrogate : SerializationSurrogate<Vector3>
    {
        protected override void GetObjectData(Vector3 vector3, SerializationInfo info)
        {
            info.AddValue("x", vector3.x);
            info.AddValue("y", vector3.y);
            info.AddValue("z", vector3.z);
        }

        protected override Vector3 SetObjectData(Vector3 vector3, SerializationInfo info)
        {
            vector3.x = info.GetSingle("x");
            vector3.y = info.GetSingle("y");
            vector3.z = info.GetSingle("z");
            return vector3;
        }
    }
}
