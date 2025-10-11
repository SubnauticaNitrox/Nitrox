using Newtonsoft.Json;
using Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.Catalog;
using Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.JSON;

namespace Nitrox.Server.Subnautica.Models.Resources.AddressablesTools
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
