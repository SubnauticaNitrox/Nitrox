using NitroxModel.DataStructures.Util;
using System;
using UnityEngine;

namespace NitroxClient
{
    public class ApiHelper
    {
        public static NitroxModel.DataStructures.Vector3 Vector3(Vector3 vector3)
        {
            return new NitroxModel.DataStructures.Vector3(vector3.x, vector3.y, vector3.z);
        }

        public static Vector3 Vector3(NitroxModel.DataStructures.Vector3 vector3)
        {
            return new Vector3(vector3.X, vector3.Y, vector3.Z);
        }

        public static NitroxModel.DataStructures.Quaternion Quaternion(Quaternion quaternion)
        {
            return new NitroxModel.DataStructures.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public static Quaternion Quaternion(NitroxModel.DataStructures.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static NitroxModel.DataStructures.Int3 Int3(Int3 int3)
        {
            return new NitroxModel.DataStructures.Int3(int3.x, int3.y, int3.z);
        }

        public static Int3 Int3(NitroxModel.DataStructures.Int3 int3)
        {
            return new Int3(int3.X, int3.Y, int3.Z);
        }

        public static String TechType(TechType techType)
        {
            return Enum.GetName(typeof(TechType), techType);
        }

        public static Optional<TechType> TechType(String techTypeString)
        {
            TechType techType;
            UWE.Utils.TryParseEnum(techTypeString, out techType);

            return Optional<TechType>.OfNullable(techType);
        }

        public static Int3 Int3(Vector3 v)
        {
            return new Int3((int)v.x, (int)v.y, (int)v.z);
        }

        public static NitroxModel.DataStructures.Transform Transform(Transform transform)
        {
            return new NitroxModel.DataStructures.Transform(Vector3(transform.position), Vector3(transform.localScale), Quaternion(transform.rotation));
        }

        public static void SetTransform(Transform transform, NitroxModel.DataStructures.Transform transform2)
        {
            transform.position = Vector3(transform2.Position);
            transform.localScale = Vector3(transform2.Scale);
            transform.rotation = Quaternion(transform2.Rotation);
        }
    }
}
