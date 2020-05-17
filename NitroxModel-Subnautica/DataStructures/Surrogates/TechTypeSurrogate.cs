using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Surrogates;

namespace NitroxModel_Subnautica.DataStructures.Surrogates
{
    public class TechTypeSurrogate : SerializationSurrogate<NitroxTechType>
    {
        protected override void GetObjectData(NitroxTechType techType, SerializationInfo info)
        {
            info.AddValue("name", techType.Name);
        }

        protected override NitroxTechType SetObjectData(NitroxTechType techType, SerializationInfo info)
        {
            techType.Name = info.GetString("name");
            return techType;
        }
    }
}
