using System.Collections.Generic;
using AssetsTools.NET;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers
{
    public class TransformAssetParser : AssetParser
    {
        public static Dictionary<AssetIdentifier, TransformAsset> TransformsByAssetId { get; } = new Dictionary<AssetIdentifier, TransformAsset>();
        public static Dictionary<AssetIdentifier, AssetIdentifier> ChildrenIdToParentId { get; } = new Dictionary<AssetIdentifier, AssetIdentifier>();

        public override void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            TransformAsset transformAsset = new TransformAsset();
            transformAsset.Identifier = identifier;

            reader.Position += 12;

            transformAsset.LocalRotation = new NitroxQuaternion(
                reader.ReadSingle(), // Quaternion X
                reader.ReadSingle(), // Quaternion Y
                reader.ReadSingle(), // Quaternion Z
                reader.ReadSingle()); // Quaternion W

            transformAsset.LocalPosition = new NitroxVector3(
               reader.ReadSingle(), // Position X
               reader.ReadSingle(), // Position Y
               reader.ReadSingle()); // Position Z

            transformAsset.LocalScale = new NitroxVector3(
               reader.ReadSingle(), // Scale X
               reader.ReadSingle(), // Scale Y
               reader.ReadSingle()); // Scale Z

            int childrenCount = reader.ReadInt32();

            for (int i = 0; i < childrenCount; i++)
            {
                AssetIdentifier child = new AssetIdentifier(reader.ReadInt32(), reader.ReadInt64());
                ChildrenIdToParentId.Add(child, identifier);
            }

            transformAsset.ParentIdentifier = new AssetIdentifier(reader.ReadInt32(), reader.ReadInt64());

            TransformsByAssetId.Add(identifier, transformAsset);
        }
    }
}
