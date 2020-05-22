using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProtoBufNet;

namespace NitroxModel.DataStructures.JsonConverter
{
    public class AttributeContractResolver : DefaultContractResolver
    {
        //Using Protobuf attributes for json
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            return properties.Where(p => p.AttributeProvider.GetAttributes(true).OfType<ProtoMemberAttribute>().Any()).ToList();
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
