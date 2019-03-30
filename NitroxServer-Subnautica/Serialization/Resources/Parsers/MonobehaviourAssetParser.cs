using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours;
using static NitroxServer_Subnautica.Serialization.Resources.Parsers.MonoscriptAssetParser;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers
{
    public class MonobehaviourAssetParser : AssetParser
    {
        private Dictionary<string, AssetParser> monobehaviourParsersByMonoscriptName = new Dictionary<string, AssetParser>()
        {
            { "WorldEntityData", new WorldEntityDataParser() },
            { "PrefabPlaceholdersGroup", new PrefabPlaceholdersGroupParser() },
            { "PrefabIdentifier", new PrefabIdentifierParser() }
        };
        
        public override void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            MonobehaviourAsset monobehaviour = new MonobehaviourAsset();

            monobehaviour.GameObjectIdentifier = new AssetIdentifier((uint)reader.ReadInt32(), (ulong)reader.ReadInt64());
            monobehaviour.Enabled = reader.ReadInt32(); // unknown but assume this is what it is
            monobehaviour.MonoscriptIdentifier = new AssetIdentifier((uint)reader.ReadInt32(), (ulong)reader.ReadInt64());
            monobehaviour.Name = reader.ReadCountStringInt32();

            // Hack - If we have not yet loaded monoscripts then we are currently processing unit monobehaviours 
            // that we do not care about.  Monoscripts should be fully loaded before we actually parse anything
            // we do care about in resource.assets.  If this becomes a problem later, we can do two passes and
            // load monobeahviours in the second pass.
            if(!MonoscriptAssetParser.MonoscriptsByAssetId.ContainsKey(monobehaviour.MonoscriptIdentifier))
            {
                return;
            }

            MonoscriptAsset monoscript = MonoscriptAssetParser.MonoscriptsByAssetId[monobehaviour.MonoscriptIdentifier];
            monobehaviour.MonoscriptName = monoscript.Name;

            AssetParser monoResourceParser;

            if (monobehaviourParsersByMonoscriptName.TryGetValue(monoscript.Name, out monoResourceParser))
            {
                monoResourceParser.Parse(monobehaviour.GameObjectIdentifier, reader, resourceAssets);
            }
        }
    }
}
