using AddressablesTools.Classes;
using System;
using System.IO;
using System.Runtime.InteropServices;
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
                        Hash128 hash = new Hash128(str);
                        return hash;
                    }

                case ObjectType.Type:
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        throw new NotSupportedException($"{nameof(ObjectType)}.{nameof(ObjectType.Type)} is only supported on windows because it uses {nameof(Type.GetTypeFromCLSID)}");
                    }
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

        internal static void Encode(BinaryWriter bw, object ob)
        {
            switch (ob)
            {
                case string str:
                    {
                        byte[] asciiEncoding = Encoding.ASCII.GetBytes(str);
                        string asciiText = Encoding.ASCII.GetString(asciiEncoding);
                        if (str != asciiText)
                        {
                            bw.Write((byte)ObjectType.UnicodeString);
                            WriteString4Unicode(bw, str);
                        }
                        else
                        {
                            bw.Write((byte)ObjectType.AsciiString);
                            WriteString4(bw, str);
                        }
                        break;
                    }

                case ushort ush:
                    {
                        bw.Write((byte)ObjectType.UInt16);
                        bw.Write(ush);
                        break;
                    }

                case uint uin:
                    {
                        bw.Write((byte)ObjectType.UInt32);
                        bw.Write(uin);
                        break;
                    }

                case int i:
                    {
                        bw.Write((byte)ObjectType.Int32);
                        bw.Write(i);
                        break;
                    }

                case Hash128 hash:
                    {
                        bw.Write((byte)ObjectType.Hash128);
                        bw.Write(hash.Value);
                        break;
                    }

                case TypeReference type:
                    {
                        bw.Write((byte)ObjectType.Type);
                        WriteString1(bw, type.Clsid);
                        break;
                    }

                case ClassJsonObject jsonObject:
                    {
                        bw.Write((byte)ObjectType.JsonObject);
                        WriteString1(bw, jsonObject.AssemblyName);
                        WriteString1(bw, jsonObject.ClassName);
                        WriteString4Unicode(bw, jsonObject.JsonText);
                        break;
                    }

                default:
                    {
                        throw new Exception($"Type {ob.GetType().FullName} not supported!");
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

        private static void WriteString1(BinaryWriter bw, string str)
        {
            if (str.Length > 255)
                throw new ArgumentException("String length cannot be greater than 255.");

            byte[] bytes = Encoding.ASCII.GetBytes(str);
            bw.Write((byte)bytes.Length);
            bw.Write(bytes);
        }

        private static void WriteString4(BinaryWriter bw, string str)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            bw.Write(bytes.Length);
            bw.Write(bytes);
        }

        private static void WriteString4Unicode(BinaryWriter bw, string str)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            bw.Write(bytes.Length);
            bw.Write(bytes);
        }
    }
}
