using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using NitroxModel.Platforms.OS.Shared;
using ProtoBufNet;
using ProtoBufNet.Meta;

namespace NitroxServer.Serialization;

public class ServerProtoBufSerializer : IServerSerializer
{
    public const string FILE_ENDING = ".nitrox";

    protected RuntimeTypeModel Model { get; } = TypeModel.Create();

    public ServerProtoBufSerializer(params string[] assemblies)
    {
        foreach (string assembly in assemblies)
        {
            RegisterAssemblyClasses(assembly);
        }
    }

    public string FileEnding => FILE_ENDING;

    public void Serialize(Stream stream, object o)
    {
        Model.SerializeWithLengthPrefix(stream, o, o.GetType(), PrefixStyle.Base128, 0);
    }

    public void Serialize(string filePath, object o)
    {
        string tmpPath = Path.ChangeExtension(filePath, ".tmp");
        using (Stream stream = File.OpenWrite(tmpPath))
        {
            Serialize(stream, o);
        }

        FileSystem.Instance.ReplaceFile(tmpPath, filePath);
    }

    public T Deserialize<T>(Stream stream)
    {
        T t = Activator.CreateInstance<T>();
        Model.DeserializeWithLengthPrefix(stream, t, typeof(T), PrefixStyle.Base128, 0);
        return t;
    }

    public T Deserialize<T>(string filePath)
    {
        using Stream stream = File.OpenRead(filePath);
        return Deserialize<T>(stream);
    }

    public void Deserialize(Stream stream, object o, Type t)
    {
        Model.DeserializeWithLengthPrefix(stream, o, t, PrefixStyle.Base128, 0);
    }

    private void RegisterAssemblyClasses(string assemblyName)
    {
        bool HasNitroxDataContract(Type type) => type.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0;
        bool HasNitroxProtoBuf(Type type) => type.GetCustomAttributes(typeof(ProtoContractAttribute), false).Length > 0;
        
        foreach (Type type in Assembly.Load(assemblyName).GetTypes())
        {
            try
            {
                if (HasNitroxDataContract(type) || HasNitroxProtoBuf(type))
                {
                    // As of the latest protobuf update they will automatically register detected attributes.
                    Model.Add(type, true);
                }
                else if (HasUweProtoContract(type))
                {
                    Model.Add(type, true);

                    ManuallyRegisterUweProtoMembers(type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), type);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"ServerProtoBufSerializer has thrown an error registering the type: {type} from {assemblyName}");
            }
        }
    }

    private static bool HasUweProtoContract(Type type)
    {
        foreach (object o in type.GetCustomAttributes(false))
        {
            if (o.GetType().ToString().Contains(nameof(ProtoContractAttribute)))
            {
                return true;
            }
        }

        return false;
    }

    private void ManuallyRegisterUweProtoMembers(MemberInfo[] info, Type type)
    {
        foreach (MemberInfo property in info)
        {
            if (property.DeclaringType != type)
            {
                continue;
            }

            foreach (object customAttribute in property.GetCustomAttributes(false))
            {
                Type attributeType = customAttribute.GetType();
                if (attributeType.ToString().Contains(nameof(ProtoMemberAttribute)))
                {
                    int tag = (int)attributeType.GetProperty(nameof(ProtoMemberAttribute.Tag), BindingFlags.Public | BindingFlags.Instance).GetValue(customAttribute, Array.Empty<object>());
                    Model[type].Add(tag, property.Name);
                }
            }
        }
    }
}
