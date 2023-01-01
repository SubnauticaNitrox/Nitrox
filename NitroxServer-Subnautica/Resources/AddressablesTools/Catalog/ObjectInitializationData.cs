using AddressablesTools.JSON;

namespace AddressablesTools.Catalog
{
    public class ObjectInitializationData
    {
        public string Id { get; set; }
        public SerializedType ObjectType { get; set; }
        public string Data { get; set; }

        internal void Read(ObjectInitializationDataJson obj)
        {
            Id = obj.m_Id;
            ObjectType = new SerializedType();
            ObjectType.Read(obj.m_ObjectType);
            Data = obj.m_Data;
        }
    }
}
