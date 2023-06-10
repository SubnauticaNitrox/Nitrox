using System.IO;
using AddressablesTools.Catalog;

namespace AddressablesTools.Binary
{
    internal class ObjectInitializationDataBinary : ObjectInitializationData
    {
        public static ObjectInitializationDataBinary ReadBinary(BinaryReader reader)
        {
            return new ObjectInitializationDataBinary
            {
                Id = reader.ReadString(),
                ObjectType = SerializedTypeBinary.ReadBinary(reader),
                Data = reader.ReadString()
            };
        }
    }
}
