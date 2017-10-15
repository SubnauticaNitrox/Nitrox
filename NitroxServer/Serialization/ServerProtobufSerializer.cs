using ProtoBufNet.Meta;
using System;
using System.Reflection;
using NitroxModel.Logger;
using ProtoBufNet;
using System.IO;

namespace NitroxServer.Serialization
{
    class ServerProtobufSerializer
    {
        private RuntimeTypeModel model;

        public ServerProtobufSerializer()
        {
            model = TypeModel.Create();
            RegisterAssemblyClasses("Assembly-CSharp");
            RegisterAssemblyClasses("Assembly-CSharp-firstpass");
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

        private void RegisterAssemblyClasses(String assemblyName)
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
                foreach (var customAttribute in property.GetCustomAttributes(true))
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
