using AddressablesTools.Catalog;
using AddressablesTools.JSON;
using Newtonsoft.Json;

namespace AddressablesTools
{
    public static class AddressablesJsonParser
    {
        internal static ContentCatalogDataJson CCDJsonFromString(string data)
        {
            return JsonConvert.DeserializeObject<ContentCatalogDataJson>(data);
        }

        public static ContentCatalogData FromString(string data)
        {
            ContentCatalogDataJson ccdJson = CCDJsonFromString(data);

            ContentCatalogData catalogData = new ContentCatalogData();
            catalogData.Read(ccdJson);

            return catalogData;
        }
    }
}
