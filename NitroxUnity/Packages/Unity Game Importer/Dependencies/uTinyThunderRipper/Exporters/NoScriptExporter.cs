using System;
using System.Collections.Generic;
using uTinyRipper;
using uTinyRipper.Classes;
using uTinyRipper.Converters;
using uTinyRipper.Project;
using uTinyRipper.SerializedFiles;
using Object = uTinyRipper.Classes.Object;

namespace ThunderKit.uTinyRipper
{
	internal class NoScriptExporter : IAssetExporter
	{
		public bool IsHandle(Object asset, ExportOptions options)
		{
			return true;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new ScriptExportCollection(this, (MonoScript)asset);
		}

		public bool Export(IExportContainer container, Object asset, string path)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string dirPath)
		{
			Export(container, assets, dirPath, null);
			return true;
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string dirPath, Action<IExportContainer, Object, string> callback)
		{
		}

		public AssetType ToExportType(Object asset)
		{
			return AssetType.Meta;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}