using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.Surrogates
{
    namespace NitroxModel.DataStructures.Surrogates
    {
        public class Int3Surrogate : SerializationSurrogate<Int3>
        {
            protected override void GetObjectData(Int3 int3, SerializationInfo info)
            {
                info.AddValue("x", int3.x);
                info.AddValue("y", int3.y);
                info.AddValue("z", int3.z);
            }

            protected override Int3 SetObjectData(Int3 int3, SerializationInfo info)
            {
                int3.x = info.GetInt32("x");
                int3.y = info.GetInt32("y");
                int3.z = info.GetInt32("z");
                return int3;
            }
        }
    }
}
