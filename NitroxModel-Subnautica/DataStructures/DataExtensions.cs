using System;
using UnityEngine;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.DataStructures
{
    /// <summary>
    ///     Extension methods to convert between Unity and Nitrox' data transfer objects (DTOs).
    /// </summary>
    public static class DataExtensions
    {
        public static DTO.TechType ToDto(this TechType o)
        {
            return new DTO.TechType(o.ToString());
        }

        public static TechType ToUnity(this DTO.TechType obj)
        {
            return (TechType)Enum.Parse(typeof(TechType), obj.Name);
        }

        public static Quaternion ToUnity(this DTO.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static Vector3 ToUnity(this DTO.Vector3 vector3)
        {
            return new Vector3(vector3.X, vector3.Y, vector3.Z);
        }

        public static Vector3[] ToUnity(this DTO.Vector3[] vector3Array)
        {
            if (vector3Array == null)
            {
                return new Vector3[0];
            }
            Vector3[] result = new Vector3[vector3Array.Length];
            for (int i = 0; i < vector3Array.Length; i++)
            {
                result[i] = vector3Array[i].ToUnity();
            }
            return result;
        }

        public static Color ToUnity(this DTO.Color color)
        {
            return new Color(color.R, color.G, color.B);
        }

        public static DTO.Vector3[] ToDto(this Vector3[] vector3Array)
        {
            if (vector3Array == null)
            {
                return new DTO.Vector3[0];
            }
            DTO.Vector3[] result = new DTO.Vector3[vector3Array.Length];
            for (int i = 0; i < vector3Array.Length; i++)
            {
                result[i] = vector3Array[i].ToDto();
            }
            return result;
        }

        public static DTO.Quaternion ToDto(this Quaternion quaternion)
        {
            return new DTO.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public static DTO.Vector3 ToDto(this Vector3 vector3)
        {
            return new DTO.Vector3(vector3.x, vector3.y, vector3.z);
        }

        public static DTO.Color ToDto(this Color color)
        {
            return new DTO.Color(color.r, color.g, color.b);
        }
    }
}
