using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Surrogates;
using ProtoBuf;
using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.Surrogates
{
    [ProtoContract]
    public class QuaternionSurrogate : SerializationSurrogate<Quaternion>
    {
        [ProtoMember(1)]
        public float X { get; private set; }

        [ProtoMember(2)]
        public float Y { get; private set; }

        [ProtoMember(3)]
        public float Z { get; private set; }

        [ProtoMember(4)]
        public float W { get; private set; }

        protected override void GetObjectData(Quaternion obj, SerializationInfo info)
        {
            info.AddValue("w", obj.w);
            info.AddValue("x", obj.x);
            info.AddValue("y", obj.y);
            info.AddValue("z", obj.z);
        }

        protected override Quaternion SetObjectData(Quaternion obj, SerializationInfo info)
        {
            obj.w = info.GetSingle("w");
            obj.x = info.GetSingle("x");
            obj.y = info.GetSingle("y");
            obj.z = info.GetSingle("z");
            return obj;
        }

        public static implicit operator QuaternionSurrogate(Quaternion v)
        {
            return new QuaternionSurrogate
            {
                X = v.x,
                Y = v.y,
                W = v.w,
                Z = v.z
            };
        }

        public static implicit operator Quaternion(QuaternionSurrogate surrogate)
        {
            return new Quaternion(surrogate.X, surrogate.Y, surrogate.Z, surrogate.W);
        }

        public static implicit operator QuaternionSurrogate(NitroxQuaternion v)
        {
            return new QuaternionSurrogate
            {
                X = v.X,
                Y = v.Y,
                W = v.W,
                Z = v.Z
            };
        }

        public static implicit operator NitroxQuaternion(QuaternionSurrogate surrogate)
        {
            return new NitroxQuaternion(surrogate.X, surrogate.Y, surrogate.Z, surrogate.W);
        }
    }
}
