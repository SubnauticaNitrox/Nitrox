using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.Surrogates
{
    namespace NitroxModel.DataStructures.Surrogates
    {
        public class Int3Surrogate : SerializationSurrogate<Int3>
        {
            protected override void GetObjectData(Int3 quaternion, SerializationInfo info)
            {
                info.AddValue("x", quaternion.x);
                info.AddValue("y", quaternion.y);
                info.AddValue("z", quaternion.z);
            }

            protected override Int3 SetObjectData(Int3 quaternion, SerializationInfo info)
            {
                quaternion.x = info.GetInt32("x");
                quaternion.y = info.GetInt32("y");
                quaternion.z = info.GetInt32("z");
                return quaternion;
            }
        }
    }
}
