using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ProtoBuf;
using ProtoBuf.Meta;

namespace NitroxClient.Helpers
{
    public class NitroxProtobufSerializer
    {
        private readonly RuntimeTypeModel model;
        private readonly Dictionary<Type, int> knownTypes;

        public static NitroxProtobufSerializer Main;

        protected RuntimeTypeModel Model { get { return model; } }

        public NitroxProtobufSerializer(params string[] assemblies)
        {
            Main = this;
            model = TypeModel.Create();
            knownTypes = (Dictionary<Type, int>)typeof(ProtobufSerializerPrecompiled).GetField("knownTypes", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

            foreach (string assembly in assemblies)
            {
                RegisterAssemblyClasses(assembly);
            }

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                bool hasUweProtobuf = (type.GetCustomAttributes(typeof(ProtoContractAttribute), true).Length > 0);

                if (hasUweProtobuf)
                {
                    // As of the latest protobuf update they will automatically register detected attributes.
                    model.Add(type, true);
                    knownTypes[type] = int.MaxValue; // UWE precompiled is going to pass everything to us
                }
            }
        }

        public void Serialize(Stream stream, object o)
        {
            model.SerializeWithLengthPrefix(stream, o, o.GetType(), PrefixStyle.Base128, 0);
        }

        public void Serialize(ProtoWriter writer, object o)
        {
            Stream stream = (Stream)writer.GetType().GetField("dest", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(writer); // really...
            model.SerializeWithLengthPrefix(stream, o, o.GetType(), PrefixStyle.Base128, 0);
        }

        public T Deserialize<T>(Stream stream)
        {
            T t = (T)Activator.CreateInstance(typeof(T));
            return (T)Deserialize(stream, t, typeof(T));
        }

        public object Deserialize(ProtoReader reader, object o, Type t)
        {
            Stream stream = (Stream)reader.GetType().GetField("_source", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(reader); // really...
            return Deserialize(stream, o, t);
        }

        public object Deserialize(Stream stream, object o, Type t)
        {
            return model.DeserializeWithLengthPrefix(stream, o, t, PrefixStyle.Base128, 0);
        }

        private void RegisterAssemblyClasses(string assemblyName)
        {
            foreach (Type type in Assembly.Load(assemblyName).GetTypes())
            {
                bool hasUweProtobuf = (type.GetCustomAttributes(typeof(ProtoContractAttribute), true).Length > 0);

                if (hasUweProtobuf)
                {
                    // As of the latest protobuf update they will automatically register detected attributes.
                    model.Add(type, true);
                    knownTypes[type] = int.MaxValue; // UWE precompiled is going to pass everything to us
                }
                else if (HasNitroxProtoContract(type))
                {
                    model.Add(type, true);
                    knownTypes[type] = int.MaxValue; // UWE precompiled is going to pass everything to us

                    ManuallyRegisterNitroxProtoMembers(type.GetFields(), type);
                    ManuallyRegisterNitroxProtoMembers(type.GetProperties(), type);
                }
            }
        }

        private bool HasNitroxProtoContract(Type type)
        {
            foreach (object o in type.GetCustomAttributes(true))
            {
                if (o.GetType().ToString().Contains("ProtoContractAttribute"))
                {
                    return true;
                }
            }

            return false;
        }

        private void ManuallyRegisterNitroxProtoMembers(MemberInfo[] info, Type type)
        {
            foreach (MemberInfo property in info)
            {
                foreach (object customAttribute in property.GetCustomAttributes(true))
                {
                    Type attributeType = customAttribute.GetType();

                    if (attributeType.ToString().Contains("ProtoMemberAttribute"))
                    {
                        int tag = (int)attributeType.GetProperty("Tag", BindingFlags.Public | BindingFlags.Instance).GetValue(customAttribute, new object[] { });
                        model[type].Add(tag, property.Name);
                    }
                }
            }
        }
    }
}
