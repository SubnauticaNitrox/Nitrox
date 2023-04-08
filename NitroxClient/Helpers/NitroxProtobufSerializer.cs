using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;

namespace NitroxClient.Helpers
{
    public class NitroxProtobufSerializer
    {
        public readonly RuntimeTypeModel model;
        public readonly Dictionary<Type, int> NitroxTypes = new Dictionary<Type, int>();

        protected RuntimeTypeModel Model => model;

        public NitroxProtobufSerializer(params string[] assemblies)
        {
            model = TypeModel.Create();

            foreach (string assembly in assemblies)
            {
                RegisterAssemblyClasses(assembly);
            }

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                bool hasUweProtobuf = (type.GetCustomAttributes(typeof(ProtoContractAttribute), true).Length > 0);

                if (hasUweProtobuf)
                {
                    AddType(type);
                }
            }
        }

        private void AddType(Type type)
        {
            // As of the latest protobuf update they will automatically register detected attributes.
            model.Add(type, true);
            ProtobufSerializerPrecompiled.knownTypes[type] = int.MaxValue; // UWE precompiled is going to pass everything to us
            NitroxTypes[type] = int.MaxValue;

            if (type.IsSubclassOf(typeof(MonoBehaviour))) // Add Nitrox MonoBehaviours to the Component whitelist
            {
                ProtobufSerializer.componentWhitelist.Add(type.FullName);
            }
        }

        public void Serialize(Stream stream, object o)
        {
            model.SerializeWithLengthPrefix(stream, o, o.GetType(), PrefixStyle.Base128, 0);
        }

        public T Deserialize<T>(Stream stream)
        {
            T t = (T)Activator.CreateInstance(typeof(T));
            return (T)Deserialize(stream, t, typeof(T));
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
                    AddType(type);
                }
                else if (HasNitroxProtoContract(type))
                {
                    AddType(type);

                    ManuallyRegisterNitroxProtoMembers(type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), type);
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
                foreach (object customAttribute in property.GetCustomAttributes(false))
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
