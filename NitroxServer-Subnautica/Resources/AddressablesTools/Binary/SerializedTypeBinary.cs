using System.IO;
using AddressablesTools.Catalog;

namespace AddressablesTools.Binary
{
    internal class SerializedTypeBinary : SerializedType
    {
        public static SerializedTypeBinary ReadBinary(BinaryReader reader)
        {
            return new SerializedTypeBinary
            {
                AssemblyName = reader.ReadString(),
                ClassName = reader.ReadString()
            };
        }
    }
}
