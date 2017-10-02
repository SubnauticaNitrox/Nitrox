using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.Surrogates
{
    public class TechTypeSurrogate : SerializationSurrogate<TechType>
    {
        protected override void GetObjectData(TechType obj, SerializationInfo info)
        {
            info.AddValue("TechType", (int)obj);
        }

        protected override TechType SetObjectData(TechType obj, SerializationInfo info)
        {
            return obj = (TechType)info.GetInt32("TechType");
        }
    }
}
