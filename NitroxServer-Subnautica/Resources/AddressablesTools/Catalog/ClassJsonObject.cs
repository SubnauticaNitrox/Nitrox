namespace AddressablesTools.Catalog
{
    public class ClassJsonObject
    {
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public string JsonText { get; set; }

        public ClassJsonObject(string assemblyName, string className, string jsonText)
        {
            AssemblyName = assemblyName;
            ClassName = className;
            JsonText = jsonText;
        }
    }
}
