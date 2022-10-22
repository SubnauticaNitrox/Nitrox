using System;
using System.IO;
using System.Text;

namespace AddressablesTools.Catalog
{
    internal static class SerializedObjectDecoder
    {
        internal enum ObjectType
        {
            AsciiString,
            UnicodeString,
            UInt16,
            UInt32,
            Int32,
            Hash128,
            Type,
            JsonObject
        }

        internal static object Decode(BinaryReader br)
        {
            ObjectType type = (ObjectType)br.ReadByte();
            
            switch (type)
            {
                case ObjectType.AsciiString:
                {
                    string str = ReadString4(br);
                    return str;
                }

                case ObjectType.UnicodeString:
                {
                    string str = ReadString4Unicode(br);
                    return str;
                }

                case ObjectType.UInt16:
                {
                    return br.ReadUInt16();
                }

                case ObjectType.UInt32:
                {
                    return br.ReadUInt32();
                }

                case ObjectType.Int32:
                {
                    return br.ReadInt32();
                }

                case ObjectType.Hash128:
                {
                    // read as string for now
                    string str = ReadString1(br);
                    return str;
                }

                case ObjectType.Type:
                {
                    string str = ReadString1(br);
                    return Type.GetTypeFromCLSID(new Guid(str));
                }

                case ObjectType.JsonObject:
                {
                    string assemblyName = ReadString1(br);
                    string className = ReadString1(br);
                    string jsonText = ReadString4Unicode(br);
                    ClassJsonObject jsonObj = new ClassJsonObject(assemblyName, className, jsonText);
                    return jsonObj;
                }

                default:
                {
                    return null;
                }
            }
        }

        private static string ReadString1(BinaryReader br)
        {
            int length = br.ReadByte();
            string str = Encoding.ASCII.GetString(br.ReadBytes(length));
            return str;
        }

        private static string ReadString4(BinaryReader br)
        {
            int length = br.ReadInt32();
            string str = Encoding.ASCII.GetString(br.ReadBytes(length));
            return str;
        }

        private static string ReadString4Unicode(BinaryReader br)
        {
            int length = br.ReadInt32();
            string str = Encoding.Unicode.GetString(br.ReadBytes(length));
            return str;
        }
    }
}
