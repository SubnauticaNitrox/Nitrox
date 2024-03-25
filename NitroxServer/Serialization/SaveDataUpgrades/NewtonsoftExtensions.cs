#nullable enable
using System;
using Newtonsoft.Json.Linq;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxServer.Serialization.SaveDataUpgrades
{
    public static class NewtonsoftExtensions
    {
        public static void Rename(this JToken token, string newName)
        {
            if (token == null)
            {
                //PrintStatusCode(StatusCode.INVALID_VARIABLE_VAL, "token" + "Cannot rename a null token");
            }

            JProperty property;

            if (token.Type == JTokenType.Property)
            {
                if (token.Parent == null)
                {
                    //PrintStatusCode(StatusCode.INVALID_VARIABLE_VAL, "Cannot rename a property with no parent");
                }

                property = (JProperty)token;
            }
            else
            {
                if (token.Parent == null || token.Parent.Type != JTokenType.Property)
                {
                    //PrintStatusCode(StatusCode.INVALID_VARIABLE_VAL, "This token's parent is not a JProperty; cannot rename");
                }

                property = (JProperty)token.Parent;
            }

            // Note: to avoid triggering a clone of the existing property's value,
            // we need to save a reference to it and then null out property.Value
            // before adding the value to the new JProperty.  
            // Thanks to @dbc for the suggestion.

            JToken? existingValue = property.Value;
            property.Value = null!;
            JProperty? newProperty = new JProperty(newName, existingValue);
            property.Replace(newProperty);
        }
    }
}
