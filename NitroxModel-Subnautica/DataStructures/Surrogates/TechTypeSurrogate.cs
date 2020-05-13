using System.Runtime.Serialization;
using NitroxModel.DataStructures.Surrogates;
using TechTypeModel = NitroxModel.DataStructures.TechType;

namespace NitroxModel_Subnautica.DataStructures.Surrogates
{
    public class TechTypeSurrogate : SerializationSurrogate<TechTypeModel>
    {
        protected override void GetObjectData(TechTypeModel techType, SerializationInfo info)
        {
            info.AddValue("name", techType.Name);
        }

        protected override TechTypeModel SetObjectData(TechTypeModel techType, SerializationInfo info)
        {
            techType.Name = info.GetString("name");
            return techType;
        }
    }
}
