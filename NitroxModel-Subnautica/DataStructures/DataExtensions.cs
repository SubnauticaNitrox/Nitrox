using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures
{
    /// <summary>
    ///     Contains extension methods for converting Unity types to Nitrox types and vice versa.
    ///     This is necessary to allow the server to work without a dependency on game models while still being able to represent game models.
    /// </summary>
    /// <remarks>
    ///     The abbreviation "DTO" means Data Transfer Object which is a common term used in REST APIs.
    ///     Its purpose is to be a serializable type and to transfer the least amount of data necessary per request (e.g. API call). 
    /// </remarks>
    public static class DataExtensions
    {
        public static Int3 ToUnity(this NitroxInt3 v)
        {
            return new Int3(v.X, v.Y, v.Z);
        }

        public static NitroxInt3 ToDto(this Int3 v)
        {
            return new NitroxInt3(v.x, v.y, v.z);
        }

        public static NitroxVector3 ToDto(this Vector3 v)
        {
            return new NitroxVector3(v.x, v.y, v.z);
        }

        public static Vector3 ToUnity(this NitroxVector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static NitroxVector4 ToDto(this Vector4 v)
        {
            return new NitroxVector4(v.x, v.y, v.z, v.w);
        }

        public static Vector4 ToUnity(this NitroxVector4 v)
        {
            return new Vector4(v.X, v.Y, v.Z, v.W);
        }

        public static NitroxTechType ToDto(this TechType v)
        {
            return new NitroxTechType(v.ToString());
        }

        public static TechType ToUnity(this NitroxTechType v)
        {
            return (TechType)Enum.Parse(typeof(TechType), v.Name);
        }

        public static Quaternion ToUnity(this NitroxQuaternion v)
        {
            return new Quaternion(v.X, v.Y, v.Z, v.W);
        }

        public static NitroxQuaternion ToDto(this Quaternion v)
        {
            return new NitroxQuaternion(v.x, v.y, v.z, v.w);
        }

        public static NitroxColor ToDto(this Color v)
        {
            return new NitroxColor(v.r, v.g, v.b, v.a);
        }

        public static Color ToUnity(this NitroxColor v)
        {
            return new Color(v.R, v.G, v.B, v.A);
        }

        public static NitroxTransform ToLocalDto(this Transform transform)
        {
            return new NitroxTransform(transform.localPosition.ToDto(),
                                       transform.localRotation.ToDto(),
                                       transform.localScale.ToDto());
        }

        public static NitroxTransform ToWorldDto(this Transform transform)
        {
            return new NitroxTransform(transform.position.ToDto(),
                                       transform.rotation.ToDto(),
                                       transform.localScale.ToDto());
        }

        public static NitroxColor[] ToDto(this Color[] v)
        {
            NitroxColor[] result = new NitroxColor[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                result[i] = v[i].ToDto();
            }
            return result;
        }

        public static NitroxVector3[] ToDto(this Vector3[] v)
        {
            if (v == null)
            {
                return Array.Empty<NitroxVector3>();
            }

            NitroxVector3[] result = new NitroxVector3[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                result[i] = v[i].ToDto();
            }
            return result;
        }

        public static Vector3[] ToUnity(this NitroxVector3[] v)
        {
            if (v == null)
            {
                return Array.Empty<Vector3>();
            }

            Vector3[] result = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                result[i] = v[i].ToUnity();
            }
            return result;
        }

        public static StoryGoalExecuted.EventType ToDto(this Story.GoalType goalType)
        {
            return goalType switch
            {
                Story.GoalType.PDA => StoryGoalExecuted.EventType.PDA,
                Story.GoalType.Radio => StoryGoalExecuted.EventType.RADIO,
                Story.GoalType.Encyclopedia => StoryGoalExecuted.EventType.ENCYCLOPEDIA,
                Story.GoalType.Story => StoryGoalExecuted.EventType.STORY,
                _ => throw new ArgumentException("The provided Story.GoalType doesn't correspond to a StoryEventSend.EventType"),
            };
        }

        public static Story.GoalType ToUnity(this StoryGoalExecuted.EventType eventType)
        {
            return eventType switch
            {
                StoryGoalExecuted.EventType.PDA => Story.GoalType.PDA,
                StoryGoalExecuted.EventType.RADIO => Story.GoalType.Radio,
                StoryGoalExecuted.EventType.ENCYCLOPEDIA => Story.GoalType.Encyclopedia,
                StoryGoalExecuted.EventType.STORY => Story.GoalType.Story,
                _ => throw new ArgumentException("The provided StoryEventSend.EventType doesn't correspond to a Story.GoalType")
            };
        }

        public static PDAEntry ToDto(this PDAScanner.Entry entry)
        {
            return new(entry.techType.ToDto(), entry.unlocked);
        }

        public static PDAScanner.Entry ToUnity(this PDAEntry entry)
        {
            return new() { techType = entry.TechType.ToUnity(), unlocked = entry.Unlocked };
        }
    }
}
