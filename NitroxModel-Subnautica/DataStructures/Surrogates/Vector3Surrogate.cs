using System.Runtime.Serialization;
using NitroxModel.DataStructures.Surrogates;
using NitroxModel.DataStructures.Unity;
using ProtoBuf;
using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.Surrogates
{
    [ProtoContract]
    public class Vector3Surrogate : SerializationSurrogate<Vector3>
    {
        [ProtoMember(1)]
        public float X { get; private set; }

        [ProtoMember(2)]
        public float Y { get; private set; }

        [ProtoMember(3)]
        public float Z { get; private set; }

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

        public static implicit operator Vector3Surrogate(NitroxVector3 v)
        {
            return new Vector3Surrogate
            {
                X = v.X,
                Y = v.Y,
                Z = v.Z
            };
        }

        public static implicit operator NitroxVector3(Vector3Surrogate surrogate)
        {
            return new NitroxVector3(surrogate.X, surrogate.Y, surrogate.Z);
        }

        public static implicit operator Vector3Surrogate(Vector3 v)
        {
            return new Vector3Surrogate
            {
                X = v.x,
                Y = v.y,
                Z = v.z
            };
        }

        public static implicit operator Vector3(Vector3Surrogate surrogate)
        {
            return new Vector3(surrogate.X, surrogate.Y, surrogate.Z);
        }
    }
}
