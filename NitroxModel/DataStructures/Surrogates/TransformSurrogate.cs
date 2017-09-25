using System.Runtime.Serialization;
using UnityEngine;

namespace NitroxModel.DataStructures.Surrogates
{
    public class TransformSurrogate : SerializationSurrogate<Transform>
    {
        protected override void GetObjectData(Transform transform, SerializationInfo info)
        {
            info.AddValue("Position", transform.position);
            info.AddValue("Scale", transform.localScale);
            info.AddValue("Rotation", transform.rotation);
        }

        protected override Transform SetObjectData(Transform transform, SerializationInfo info)
        {
            if (transform == null)
            {
                transform = new GameObject().transform;
            }

            transform.position = (Vector3)info.GetValue("Position", typeof(Vector3));
            transform.localScale = (Vector3)info.GetValue("Scale", typeof(Vector3));
            transform.rotation = (Quaternion)info.GetValue("Rotation", typeof(Quaternion));

            return transform;
        }
    }
}
