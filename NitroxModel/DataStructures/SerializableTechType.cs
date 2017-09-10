using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class SerializableTechType
    {
        public TechType TechType { get {
            TechType techTypeEnum;
            UWE.Utils.TryParseEnum(techType, out techTypeEnum);
                
            return techTypeEnum;
        } }

        private String techType;

        public SerializableTechType(TechType techType)
        {
            this.techType = Enum.GetName(typeof(TechType), techType);
        }

    }
}
