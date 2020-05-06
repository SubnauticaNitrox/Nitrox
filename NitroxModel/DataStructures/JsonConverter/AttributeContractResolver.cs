using System;
using System.Collections.Generic;
using System.Linq;
using Nitrox.Newtonsoft.Json;
using Nitrox.Newtonsoft.Json.Serialization;
using ProtoBufNet;

namespace NitroxModel.DataStructures.JsonConverter
{
    public class AttributeContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            return properties.Where(p => p.AttributeProvider.GetAttributes(true).OfType<ProtoMemberAttribute>().Any()).ToList();
        }
    }

}
