using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Project;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class MetaPtr : IYAMLExportable
	{
		public MetaPtr(long fileID)
		{
			FileID = fileID;
			GUID = default;
			AssetType = default;
			UseThunderId = false;
		}

		public MetaPtr(long fileID, UnityGUID guid, AssetType assetType)
		{
			FileID = fileID;
			GUID = guid;
			AssetType = assetType;
			UseThunderId = false;
		}

		public MetaPtr(long fileID, string guid, AssetType assetType)
		{
			FileID = fileID;
			ThunderGUID = guid;
			AssetType = assetType;
			UseThunderId = true;
		}

		public MetaPtr(ClassIDType classID, AssetType assetType) :
			this(ExportCollection.GetMainExportID((uint)classID), UnityGUID.MissingReference, assetType)
		{
			UseThunderId = false;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;

			node.Add(FileIDName, FileID);
			if (UseThunderId)
			{
				node.Add(GuidName, new YAMLScalarNode(ThunderGUID));
				node.Add(TypeName, (int)AssetType);
			}
			else
			{
				if (!GUID.IsZero)
				{
					node.Add(GuidName, GUID.ExportYAML(container));
					node.Add(TypeName, (int)AssetType);
				}
			}

			return node;
		}

		public static MetaPtr NullPtr { get; } = new MetaPtr(0);

		public bool UseThunderId { get; }
		public long FileID { get; }
		public string ThunderGUID { get; }
		public UnityGUID GUID { get; }
		public AssetType AssetType { get; }

		public const string FileIDName = "fileID";
		public const string GuidName = "guid";
		public const string TypeName = "type";
	}
}
