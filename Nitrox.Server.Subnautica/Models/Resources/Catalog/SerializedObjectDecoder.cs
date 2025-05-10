using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Nitrox.Server.Subnautica.Models.Resources.Catalog;

internal static class SerializedObjectDecoder
{
    internal static object Decode(BinaryReader br)
    {
        ObjectType type = (ObjectType)br.ReadByte();

        switch (type)
        {
            case ObjectType.AsciiString:
            {
                return ReadString4(br);
            }

            case ObjectType.UnicodeString:
            {
                return ReadString4Unicode(br);
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
                return ReadString1(br);
            }

            case ObjectType.Type:
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw new NotSupportedException($"{nameof(ObjectType)}.{nameof(ObjectType.Type)} is only supported on windows because it uses {nameof(Type.GetTypeFromCLSID)}");
                }
                return Type.GetTypeFromCLSID(new Guid(ReadString1(br)));
            }

            case ObjectType.JsonObject:
            {
                string assemblyName = ReadString1(br);
                string className = ReadString1(br);
                string jsonText = ReadString4Unicode(br);
                ClassJsonObject jsonObj = new(assemblyName, className, jsonText);
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
}
