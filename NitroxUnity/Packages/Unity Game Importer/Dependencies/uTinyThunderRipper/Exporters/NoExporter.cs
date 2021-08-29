using System;
using System.Collections.Generic;
using uTinyRipper;
using uTinyRipper.Converters;
using uTinyRipper.Project;
using uTinyRipper.SerializedFiles;

namespace ThunderKit.uTinyRipper
{
    public class NoExporter : IAssetExporter
    {
        public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, global::uTinyRipper.Classes.Object asset)
        {
            return new EmptyCollection(this, asset);
        }

        public bool Export(IExportContainer container, global::uTinyRipper.Classes.Object asset, string path)
        {
            return false;
        }

        public void Export(IExportContainer container, global::uTinyRipper.Classes.Object asset, string path, Action<IExportContainer, global::uTinyRipper.Classes.Object, string> callback)
        {
        }

        public bool Export(IExportContainer container, IEnumerable<global::uTinyRipper.Classes.Object> assets, string path)
        {
            return false;
        }

        public void Export(IExportContainer container, IEnumerable<global::uTinyRipper.Classes.Object> assets, string path, Action<IExportContainer, global::uTinyRipper.Classes.Object, string> callback)
        {
        }

        public bool IsHandle(global::uTinyRipper.Classes.Object asset, ExportOptions options)
        {
            return true;
        }

        public AssetType ToExportType(global::uTinyRipper.Classes.Object asset)
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
