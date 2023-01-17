using AddressablesTools.JSON;

namespace AddressablesTools.Catalog
{
    public class SerializedType
    {
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }

        internal void Read(SerializedTypeJson type)
        {
            AssemblyName = type.m_AssemblyName;
            ClassName = type.m_ClassName;
        }
    }
}
