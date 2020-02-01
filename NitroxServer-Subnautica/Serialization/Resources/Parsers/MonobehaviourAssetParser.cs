using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours;
using static NitroxServer_Subnautica.Serialization.Resources.Parsers.MonoscriptAssetParser;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers
{
    public class MonobehaviourAssetParser : AssetParser
    {
        public static Dictionary<AssetIdentifier, MonobehaviourAsset> MonobehavioursByAssetId = new Dictionary<AssetIdentifier, MonobehaviourAsset>();

        private Dictionary<string, MonobehaviourParser> monobehaviourParsersByMonoscriptName = new Dictionary<string, MonobehaviourParser>()
        {
            { "WorldEntityData", new WorldEntityDataParser() },
            { "PrefabPlaceholder", new PrefabPlaceholderParser() },
            { "PrefabPlaceholdersGroup", new PrefabPlaceholdersGroupParser() },
            { "PrefabIdentifier", new PrefabIdentifierParser() },
            { "EntitySlot", new EntitySlotParser() }
        };
        
        public override void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            MonobehaviourAsset monobehaviour = new MonobehaviourAsset();

            monobehaviour.GameObjectIdentifier = new AssetIdentifier(reader.ReadInt32(), reader.ReadInt64());
            monobehaviour.Enabled = reader.ReadBoolean();
            reader.Align();
            monobehaviour.MonoscriptIdentifier = new AssetIdentifier(reader.ReadInt32(), reader.ReadInt64());
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

            MonobehaviourParser monoResourceParser;

            if (monobehaviourParsersByMonoscriptName.TryGetValue(monoscript.Name, out monoResourceParser))
            {
                monoResourceParser.Parse(identifier, monobehaviour.GameObjectIdentifier, reader, resourceAssets);
            }

            MonobehavioursByAssetId.Add(identifier, monobehaviour);
        }
    }
}
