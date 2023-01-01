using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace NitroxServer.Serialization.Json;

public class AttributeContractResolver : DefaultContractResolver
{
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
