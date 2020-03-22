using System;
using System.IO;
using System.Reflection;
using ProtoBufNet;
using ProtoBufNet.Meta;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.Serialization
{
    public class ServerProtobufSerializer
    {
        private readonly RuntimeTypeModel model;

        protected RuntimeTypeModel Model { get { return model; } }

        public ServerProtobufSerializer(params string[] assemblies)
        {
            model = TypeModel.Create();

            foreach(string assembly in assemblies)
            {
                RegisterAssemblyClasses(assembly);
            }

            RegisterHardCodedTypes();
        }

        public void Serialize(Stream stream, object o)
        {
            model.SerializeWithLengthPrefix(stream, o, o.GetType(), PrefixStyle.Base128, 0);
        }

        public T Deserialize<T>(Stream stream)
        {
            T t = (T)Activator.CreateInstance(typeof(T));
            model.DeserializeWithLengthPrefix(stream, t, typeof(T), PrefixStyle.Base128, 0);
            return t;
        }

        public void Deserialize(Stream stream, object o, Type t)
        {
            model.DeserializeWithLengthPrefix(stream, o, t, PrefixStyle.Base128, 0);
        }

        private void RegisterHardCodedTypes()
        {
            model.Add(typeof(UnityEngine.Light), true);
            model.Add(typeof(UnityEngine.BoxCollider), true);
            model.Add(typeof(UnityEngine.SphereCollider), true);
            model.Add(typeof(UnityEngine.MeshCollider), true);
            model.Add(typeof(UnityEngine.Vector3), false).SetSurrogate(typeof(NitroxVector3));
            model.Add(typeof(UnityEngine.Quaternion), false).SetSurrogate(typeof(NitroxQuaternion));
            model.Add(typeof(UnityEngine.Transform), false).SetSurrogate(typeof(NitroxTransform));
            model.Add(typeof(UnityEngine.GameObject), false).SetSurrogate(typeof(UnityStubs.GameObject));            
        }
        
        private void RegisterAssemblyClasses(string assemblyName)
        {
            foreach (Type type in Assembly.Load(assemblyName).GetTypes())
            {
                bool hasNitroxProtobuf = (type.GetCustomAttributes(typeof(ProtoContractAttribute), false).Length > 0);

                if (hasNitroxProtobuf)
                {
                    // As of the latest protobuf update they will automatically register detected attributes.
                    model.Add(type, true);
                }
                else if(HasUweProtoContract(type))
                {
                    model.Add(type, true);

                    ManuallyRegisterUweProtoMembers(type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), type);
                }
            }
        }

        private bool HasUweProtoContract(Type type)
        {
            foreach(object o in type.GetCustomAttributes(false))
            {
                if (o.GetType().ToString().Contains("ProtoContractAttribute"))
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
                if (!(property.DeclaringType != type))
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
}
