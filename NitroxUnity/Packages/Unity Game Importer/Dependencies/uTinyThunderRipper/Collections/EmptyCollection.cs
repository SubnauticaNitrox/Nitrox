using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using uTinyRipper;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Project;
using Object = uTinyRipper.Classes.Object;

namespace ThunderKit.uTinyRipper
{
	class EmptyCollection : ExportCollection
	{
		public override IAssetExporter AssetExporter { get; }
		public override ISerializedFile File { get; }
		public override IEnumerable<Object> Assets => m_scripts.Keys;
		public override string Name => nameof(ScriptExportCollection);

		private static readonly Regex s_unityEngine = new Regex(@"^UnityEngine(\.[0-9a-zA-Z]+)*(\.dll)?$", RegexOptions.Compiled);

		private readonly List<MonoScript> m_export = new List<MonoScript>();
		private readonly HashSet<MonoScript> m_unique = new HashSet<MonoScript>();
		private readonly Dictionary<Object, MonoScript> m_scripts = new Dictionary<Object, MonoScript>();

		Dictionary<string, Guid> AssemblyHash = new Dictionary<string, Guid>();
		Dictionary<string, long> ScriptId = new Dictionary<string, long>();

        public override TransferInstructionFlags Flags => throw new NotImplementedException();

		public EmptyCollection(IAssetExporter assetExporter, Object assetCollection)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));

			if (assetCollection is MonoScript script)
			{
				File = script.File;

				// find copies in whole project and skip them
				foreach (Object asset in script.File.Collection.FetchAssets())
				{
					if (asset.ClassID != ClassIDType.MonoScript)
					{
						continue;
					}

					MonoScript assetScript = (MonoScript)asset;
					MonoScript unique = assetScript;
					foreach (MonoScript export in m_unique)
					{
						if (assetScript.ClassName != export.ClassName)
						{
							continue;
						}
						if (assetScript.Namespace != export.Namespace)
						{
							continue;
						}
						if (assetScript.AssemblyName != export.AssemblyName)
						{
							continue;
						}

						unique = export;
						break;
					}

					m_scripts.Add(assetScript, unique);
					if (assetScript == unique)
					{
						m_unique.Add(assetScript);
						if (assetScript.IsScriptPresents())
						{
							m_export.Add(assetScript);
						}
					}
				}
			}
		}
		public override MetaPtr CreateExportPointer(Object asset, bool isLocal)
		{
			MonoScript script = m_scripts[asset];

			if (!AssemblyHash.ContainsKey(script.AssemblyNameOrigin))
				AssemblyHash[script.AssemblyNameOrigin] = Util.GetAssemblyHashGuid(script.AssemblyNameOrigin);

			return new MetaPtr(GetExportID(asset), Util.FormatAssemblyHash(AssemblyHash[script.AssemblyNameOrigin]), AssetExporter.ToExportType(asset));
		}

		public override bool Export(ProjectAssetContainer container, string dirPath) => false;

		public override long GetExportID(Object asset)
		{
			MonoScript script = m_scripts[asset];

			if (!AssemblyHash.ContainsKey(script.AssemblyNameOrigin))
				AssemblyHash[script.AssemblyNameOrigin] = Util.GetAssemblyHashGuid(script.AssemblyNameOrigin);

			var scriptKey = $"{script.AssemblyNameOrigin}{script.Namespace}{script.ClassName}";
			if (!ScriptId.ContainsKey(scriptKey))
				ScriptId[scriptKey] = Util.ComputeFileID(script.Namespace, script.ClassName);

			return ScriptId[scriptKey];
		}

		public override bool IsContains(Object asset) => false;
	}
}
