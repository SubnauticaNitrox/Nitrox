using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NitroxModel.Serialization;
using ProtoBufNet;

namespace NitroxServer.Serialization.Json.Converter
{
    public class AttributeContractResolver : DefaultContractResolver
    {
        //Using Protobuf attributes for json
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            if (Attribute.GetCustomAttribute(type, typeof (ProtoContractAttribute)) != null)
            {
                return GetSerializableMembers(type)
                       .Where(member => member.GetCustomAttribute<ProtoMemberAttribute>() != null)
                       .Select(member => CreateProperty(member, memberSerialization)).ToList();
            }

            if (Attribute.GetCustomAttribute(type, typeof (JsonContractTransitionAttribute)) != null)
            {
                return GetSerializableMembers(type)
                       .Where(member => member.GetCustomAttribute<JsonMemberTransitionAttribute>() != null)
                       .Select(member => CreateProperty(member, memberSerialization)).ToList();
            }

            return base.CreateProperties(type, memberSerialization);
        }

        //IDictionary to JsonArray
        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType.GetInterfaces().Any(i => i == typeof(IDictionary) || i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                return base.CreateArrayContract(objectType);
            }

            return base.CreateContract(objectType);
        }
    }

}
