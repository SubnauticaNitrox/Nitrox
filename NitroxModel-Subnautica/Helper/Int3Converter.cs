namespace NitroxModel_Subnautica.Helper.Int3
{
    public static class Int3Converter
    {
        public static NitroxModel.DataStructures.Int3 Model(this global::Int3 int3)
        {
            return new NitroxModel.DataStructures.Int3(int3.x, int3.y, int3.z);
        }

        public static global::Int3 Global(this NitroxModel.DataStructures.Int3 int3)
        {
            return new global::Int3(int3.X, int3.Y, int3.Z);
        }
    }
}
