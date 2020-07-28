using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Surrogates;

namespace NitroxModel_Subnautica.DataStructures.Surrogates
{
    public class TechTypeSurrogate : SerializationSurrogate<NitroxTechType>
    {
        protected override void GetObjectData(NitroxTechType obj, SerializationInfo info)
        {
            info.AddValue("name", obj.Name);
        }

        protected override NitroxTechType SetObjectData(NitroxTechType obj, SerializationInfo info)
        {
            obj.Name = info.GetString("name");
            return obj;
        }
    }
}
