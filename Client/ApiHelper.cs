using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
