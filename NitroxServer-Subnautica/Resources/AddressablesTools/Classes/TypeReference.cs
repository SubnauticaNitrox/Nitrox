using System;

namespace AddressablesTools.Classes
{
    public class TypeReference
    {
        public string Clsid { get; set; }

        public TypeReference(string clsid)
        {
            Clsid = clsid;
        }
    }
}
