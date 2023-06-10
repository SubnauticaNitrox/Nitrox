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

        internal void Write(ObjectInitializationDataJson obj)
        {
            obj.m_Id = Id;
            obj.m_ObjectType = new SerializedTypeJson();
            ObjectType.Write(obj.m_ObjectType);
            obj.m_Data = Data;
        }
    }
}
