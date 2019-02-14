using TechTypeModel = NitroxModel.DataStructures.TechType;

namespace NitroxModel_Subnautica.Helper
{
    public static class TechTypeConverter
    {
        public static TechTypeModel Model(this TechType o)
        {
            return new TechTypeModel(o.ToString());
        }

        public static TechType Enum(this TechTypeModel obj)
        {
            return (TechType)System.Enum.Parse(typeof(TechType), obj.Name);
        }
    }
}
