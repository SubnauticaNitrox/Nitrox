using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Surrogates;
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

        protected override void GetObjectData(Vector3 obj, SerializationInfo info)
        {
            info.AddValue("x", obj.x);
            info.AddValue("y", obj.y);
            info.AddValue("z", obj.z);
        }

        protected override Vector3 SetObjectData(Vector3 obj, SerializationInfo info)
        {
            obj.x = info.GetSingle("x");
            obj.y = info.GetSingle("y");
            obj.z = info.GetSingle("z");
            return obj;
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
