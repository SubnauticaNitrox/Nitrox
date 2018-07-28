﻿using System;
using System.IO;
using System.Reflection;
using NitroxModel.Logger;
using ProtoBufNet;
using ProtoBufNet.Meta;

namespace NitroxServer.Serialization
{
    class ServerProtobufSerializer
    {
        private readonly RuntimeTypeModel model;

        public ServerProtobufSerializer()
        {
            model = TypeModel.Create();
            RegisterAssemblyClasses("Assembly-CSharp");
            RegisterAssemblyClasses("Assembly-CSharp-firstpass");
            RegisterHardCodedTypes();
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
        }

        private void RegisterAssemblyClasses(string assemblyName)
        {
            foreach (Type type in Assembly.Load(assemblyName).GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ProtoBuf.ProtoContractAttribute), true).Length > 0)
                {
                    model.Add(type, true);

                    RegisterMembers(type.GetFields(), type);
                    RegisterMembers(type.GetProperties(), type);
                }
            }
        }

        private void RegisterMembers(MemberInfo[] info, Type type)
        {
            foreach (MemberInfo property in info)
            {
                foreach (object customAttribute in property.GetCustomAttributes(true))
                {
                    if (customAttribute is ProtoBuf.ProtoMemberAttribute)
                    {
                        int tag = ((ProtoBuf.ProtoMemberAttribute)customAttribute).Tag;

                        try
                        {
                            model[type].Add(tag, property.Name);
                        }
                        catch
                        {
                            Log.Warn("Couldn't load serializable attribute for " + type + " " + property.Name);
                        }
                    }
                }
            }
        }
    }
}
