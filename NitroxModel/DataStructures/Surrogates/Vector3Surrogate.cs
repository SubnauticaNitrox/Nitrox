using System.Runtime.Serialization;
using UnityEngine;

namespace NitroxModel.DataStructures.Surrogates
{
    public class Vector3Surrogate : SerializationSurrogate<Vector3>
    {
        protected override void GetObjectData(Vector3 vector, SerializationInfo info)
        {
            info.AddValue("x", vector.x);
            info.AddValue("y", vector.y);
            info.AddValue("z", vector.z);
        }

        protected override Vector3 SetObjectData(Vector3 vector, SerializationInfo info)
        {
            vector.x = info.GetSingle("x");
            vector.y = info.GetSingle("y");
            vector.z = info.GetSingle("z");
            return vector;
        }
    }
}
