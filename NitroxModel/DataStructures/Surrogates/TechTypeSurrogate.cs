using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.Surrogates
{
    public class TechTypeSurrogate : SerializationSurrogate<TechType>
    {
        protected override void GetObjectData(TechType techType, SerializationInfo info)
        {
            info.AddValue("TechType", (int)techType);
        }

        protected override TechType SetObjectData(TechType techType, SerializationInfo info)
        {
            return techType = (TechType)info.GetInt32("TechType");
        }
    }
}
