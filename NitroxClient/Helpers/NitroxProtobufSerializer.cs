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
            model = TypeModel.Create();

            foreach (string assembly in assemblies)
            {
                RegisterAssemblyClasses(assembly);
            }

            knownTypes = (Dictionary<Type, int>)typeof(ProtobufSerializerPrecompiled).GetField("knownTypes", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
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
                bool hasNitroxProtobuf = (type.GetCustomAttributes(typeof(ProtoContractAttribute), true).Length > 0);

                if (hasNitroxProtobuf)
                {
                    // As of the latest protobuf update they will automatically register detected attributes.
                    model.Add(type, true);
                    knownTypes[type] = int.MaxValue; // UWE precompiled is going to pass everything to us
                }
            }
        }
    }
}
