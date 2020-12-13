using System.Runtime.Serialization;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Surrogates;

namespace Nitrox.Model.Subnautica.DataStructures.Surrogates
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
