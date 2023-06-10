using System.IO;
using AddressablesTools.Catalog;
using AddressablesTools.Binary;

namespace AddressablesTools
{
    public static class AddressablesBinaryParser
    {
        public static ContentCatalogData FromPath(string path)
        {
            using FileStream input = File.OpenRead(path);
            using BinaryReader binaryReader = new(input);
            ContentCatalogDataBinary ccdBinary = new ContentCatalogDataBinary();
            ccdBinary.ReadBinary(binaryReader);

            return ccdBinary;
        }
    }
}
