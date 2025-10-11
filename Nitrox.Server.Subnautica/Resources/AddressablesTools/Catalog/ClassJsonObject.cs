namespace AddressablesTools.Catalog
{
    internal class ClassJsonObject
    {
        public string AssemblyName { get; }
        public string ClassName { get; }
        public string JsonText { get; }

        public ClassJsonObject(string assemblyName, string className, string jsonText)
        {
            AssemblyName = assemblyName;
            ClassName = className;
            JsonText = jsonText;
        }
    }
}
