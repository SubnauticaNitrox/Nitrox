using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.Surrogates
{
    public class Int3Surrogate : SerializationSurrogate<Int3>
    {
        protected override void GetObjectData(Int3 int3, SerializationInfo info)
        {
            info.AddValue("x", int3.X);
            info.AddValue("y", int3.Y);
            info.AddValue("z", int3.Z);
        }

        protected override Int3 SetObjectData(Int3 int3, SerializationInfo info)
        {
            int3.X = info.GetInt32("x");
            int3.Y = info.GetInt32("y");
            int3.Z = info.GetInt32("z");
            return int3;
        }
    }
}
